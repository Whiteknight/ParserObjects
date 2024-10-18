using System;
using System.Buffers;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Rule parses several parsers in order, failing if any parser fails. The results of each is
/// stored in an array and passed to a generator function to create the final result value.
/// </summary>
public static class Rule
{
    /* The Create() function helps us with type inference because downstream usages of Rule can
     * be quite messy. The Parser class implements the parser logic.
     */

    public static Parser<TInput, TOutput, TData> Create<TInput, TOutput, TData>(
        IReadOnlyList<IParser<TInput>> parsers,
        TData data,
        Func<TData, IReadOnlyList<object>, TOutput> produce,
        bool keepsArrayReference
    )
    {
        return new Parser<TInput, TOutput, TData>(parsers, data, produce, keepsArrayReference);
    }

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
    /// <param name="KeepsArrayReference"></param>
    /// <param name="Name"></param>
    public sealed record Parser<TInput, TOutput, TData>(
        IReadOnlyList<IParser<TInput>> Parsers,
        TData Data,
        Func<TData, IReadOnlyList<object>, TOutput> Produce,
        bool KeepsArrayReference,
        string Name = ""

    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);
            var outputs = KeepsArrayReference ? new object[Parsers.Count] : ArrayPool<object>.Shared.Rent(Parsers.Count);
            var result = Parse(state, outputs);
            if (!KeepsArrayReference)
                ArrayPool<object>.Shared.Return(outputs);
            return result;
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

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

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }

        private IResult<TOutput> Parse(IParseState<TInput> state, object[] outputs)
        {
            var startCheckpoint = state.Input.Checkpoint();
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
                return state.Fail(this, $"Parser {i} {name} failed");
            }

            var consumed = state.Input.Consumed - startCheckpoint.Consumed;
            var resultValue = Produce(Data, outputs);
            return state.Success(this, resultValue, consumed);
        }
    }
}
