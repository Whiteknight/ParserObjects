using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Returns unconditional failure, optionally with a helpful error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class FailParser<TInput, TOutput> : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    public FailParser(string? errorMessage = null)
    {
        ErrorMessage = errorMessage ?? "Guaranteed fail";
        Name = string.Empty;
    }

    public string Name { get; set; }
    public string ErrorMessage { get; }

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
}
