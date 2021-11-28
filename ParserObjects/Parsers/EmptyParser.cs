using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// The empty parser, consumes no input and always returns success.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class EmptyParser<TInput> : IParser<TInput>
{
    public EmptyParser()
    {
        Name = string.Empty;
    }

    public string Name { get; set; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return state.Success(this, Defaults.ObjectInstance, 0);
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);
}
