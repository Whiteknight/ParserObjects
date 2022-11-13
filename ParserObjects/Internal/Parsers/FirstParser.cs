using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
/// succeeds.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record FirstParser<TInput, TOutput>(
    IReadOnlyList<IParser<TInput, TOutput>> Parsers,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        if (Parsers.Count == 0)
            return state.Fail(this, "No parsers given");

        for (int i = 0; i < Parsers.Count - 1; i++)
        {
            var parser = Parsers[i];
            var result = parser.Parse(state);
            if (result.Success)
                return result;
        }

        return Parsers[Parsers.Count - 1].Parse(state);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Parsers;

    public override string ToString() => DefaultStringifier.ToString("First", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
