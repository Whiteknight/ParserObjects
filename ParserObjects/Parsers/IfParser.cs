using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Attempts to match a predicate condition and, invokes a specified parser on success or
/// failure.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class IfParser<TInput, TOutput> : IParser<TInput, TOutput>
{
    private readonly IParser<TInput> _predicate;
    private readonly IParser<TInput, TOutput> _onSuccess;
    private readonly IParser<TInput, TOutput> _onFail;

    public IfParser(IParser<TInput> predicate, IParser<TInput, TOutput> onSuccess, IParser<TInput, TOutput> onFail, string name = "")
    {
        Assert.ArgumentNotNull(predicate, nameof(predicate));
        Assert.ArgumentNotNull(onSuccess, nameof(onSuccess));
        Assert.ArgumentNotNull(onFail, nameof(onFail));
        _predicate = predicate;
        _onSuccess = onSuccess;
        _onFail = onFail;
        Name = name;
    }

    public string Name { get; }

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var cp = state.Input.Checkpoint();
        var result = _predicate.Parse(state);
        return Parse(state, result.Success ? _onSuccess : _onFail, cp, result.Consumed);
    }

    private static IResult<TOutput> Parse(IParseState<TInput> state, IParser<TInput, TOutput> parser, ISequenceCheckpoint cp, int predicateConsumed)
    {
        var thenResult = parser.Parse(state);
        if (thenResult.Success)
            return state.Success(parser, thenResult.Value, predicateConsumed + thenResult.Consumed, thenResult.Location);
        cp.Rewind();
        return state.Fail(parser, thenResult.ErrorMessage, thenResult.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => new IParser[] { _predicate, _onSuccess, _onFail };

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => new IfParser<TInput, TOutput>(_predicate, _onSuccess, _onFail, name);
}
