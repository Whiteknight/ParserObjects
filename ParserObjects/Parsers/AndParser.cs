using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Tests several parsers sequentially. If all of them succeed return Success. If any Fail,
/// return Failure. Consumes input but returns no explicit output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record AndParser<TInput>(
    IReadOnlyList<IParser<TInput>> Parsers,
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        foreach (var parser in Parsers)
        {
            var result = parser.Parse(state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }
        }

        var consumed = state.Input.Consumed - startCheckpoint.Consumed;
        return state.Success(this, Defaults.ObjectInstance, consumed, startCheckpoint.Location);
    }

    public IEnumerable<IParser> GetChildren() => Parsers;

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
