using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Returns unconditional failure, optionally with a helpful error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record FailParser<TInput, TOutput>(
    string ErrorMessage = "Fail",
    string Name = ""
) : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    IResult<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return state.Fail(this, ErrorMessage);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state)
        => ((IParser<TInput, TOutput>)this).Parse(state);

    IMultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        return new MultiResult<TOutput>(this, state.Input.CurrentLocation, startCheckpoint, new[]
        {
            new FailureResultAlternative<TOutput>(ErrorMessage, startCheckpoint)
        });
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
        => ((IMultiParser<TInput, TOutput>)this).Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
