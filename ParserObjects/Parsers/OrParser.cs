using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Tests several parsers sequentially, returning Success if any parser succeeds, Failure
/// otherwise. Consumes input but returns no explicit output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class OrParser<TInput> : IParser<TInput>
{
    private readonly IReadOnlyList<IParser<TInput>> _parsers;

    public OrParser(params IParser<TInput>[] parsers)
    {
        Assert.ArgumentNotNull(parsers, nameof(parsers));
        _parsers = parsers;
        Name = string.Empty;
    }

    public string Name { get; set; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        foreach (var parser in _parsers)
        {
            var result = parser.Parse(state);
            if (result.Success)
                return result;
        }

        return state.Fail(this, "None of the given parsers match");
    }

    public IEnumerable<IParser> GetChildren() => _parsers;

    public override string ToString() => DefaultStringifier.ToString(this);
}
