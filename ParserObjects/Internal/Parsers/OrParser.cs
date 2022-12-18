using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Tests several parsers sequentially, returning Success if any parser succeeds, Failure
/// otherwise. Consumes input but returns no explicit output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record OrParser<TInput>(
    IReadOnlyList<IParser<TInput>> Parsers,
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        foreach (var parser in Parsers)
        {
            var result = parser.Parse(state);
            if (result.Success)
                return result;
        }

        return state.Fail(this, "None of the given parsers match");
    }

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => Parsers;

    public override string ToString() => DefaultStringifier.ToString("Or", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
