using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches at the end of the input sequence. Fails if the input sequence is at any point
/// besides the end.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record EndParser<TInput>(
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return state.Input.IsAtEnd
            ? state.Success(this, Defaults.ObjectInstance, 0)
            : state.Fail(this, "Expected end of Input but found " + state.Input.Peek()!.ToString());
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("End", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
