using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parses a list of steps in sequence and produces a single output as a combination of outputs
/// from each step. Succeeds or fails as an atomic unit.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TData"></typeparam>
/// <param name="Parsers"></param>
/// <param name="Data"></param>
/// <param name="Produce"></param>
/// <param name="Name"></param>
public sealed record RuleParser<TInput, TOutput, TData>(
    IReadOnlyList<IParser<TInput>> Parsers,
    TData Data,
    Func<TData, IReadOnlyList<object>, TOutput> Produce,
    string Name = ""

) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var startCheckpoint = state.Input.Checkpoint();

        var outputs = new object[Parsers.Count];
        for (int i = 0; i < Parsers.Count; i++)
        {
            var result = Parsers[i].Parse(state);
            if (result.Success)
            {
                outputs[i] = result.Value;
                continue;
            }

            startCheckpoint.Rewind();
            var name = Parsers[i].Name;
            if (string.IsNullOrEmpty(name))
                name = "(Unnamed)";
            return state.Fail(this, $"Parser {i} {name} failed", result.Location);
        }

        var consumed = state.Input.Consumed - startCheckpoint.Consumed;
        return state.Success(this, Produce(Data, outputs), consumed, startCheckpoint.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var startCheckpoint = state.Input.Checkpoint();

        for (int i = 0; i < Parsers.Count; i++)
        {
            var result = Parsers[i].Match(state);
            if (result)
                continue;

            startCheckpoint.Rewind();
            return false;
        }

        // NOTE: It is possible that Produce() would have thrown an exception, in which case
        // we technically should return false. But we can't call Produce without calling .Parse()
        // on all children, which has a bunch of allocations, which goes against what Match() is
        // trying to do. So we return true.

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Parsers;

    public override string ToString() => DefaultStringifier.ToString("Rule", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
