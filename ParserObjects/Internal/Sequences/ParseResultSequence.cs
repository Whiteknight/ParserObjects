using System;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// An adaptor to change output values from an IParser into an ISequence of results.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class ParseResultSequence<TInput, TOutput> : ISequence<IResult<TOutput>>
{
    private readonly ISequence<TInput> _input;
    private readonly IParseState<TInput> _state;
    private readonly IParser<TInput, TOutput> _parser;
    private readonly Func<ResultBuilder, IResult<TOutput>>? _getEndSentinel;

    private IResult<TOutput>? _endSentinel;

    public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Func<ResultBuilder, IResult<TOutput>>? getEndSentinel, Action<string> log)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(parser, nameof(parser));
        _state = new ParseState<TInput>(input, log);
        _input = input;
        _parser = parser;
        _getEndSentinel = getEndSentinel;
        _endSentinel = null;
    }

    public struct ResultBuilder
    {
        private readonly IParseState<TInput> _state;
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Location _location;

        public ResultBuilder(IParseState<TInput> state, IParser<TInput, TOutput> parser, Location location)
        {
            _state = state;
            _parser = parser;
            _location = location;
        }

        public IResult<TOutput> Success(TOutput value)
            => _state.Success(_parser, value, 0, _location);

        public IResult<TOutput> Failure(string message)
            => _state.Fail(_parser, message);
    }

    public IResult<TOutput> GetNext() => GetNext(true);

    public IResult<TOutput> Peek() => GetNext(false);

    public Location CurrentLocation => _input.CurrentLocation;

    public bool IsAtEnd => _input.IsAtEnd;

    public int Consumed => _input.Consumed;

    public SequenceCheckpoint Checkpoint() => _input.Checkpoint();

    private IResult<TOutput> GetNext(bool advance)
    {
        if (_input.IsAtEnd)
        {
            if (_endSentinel != null)
                return _endSentinel;

            var builder = new ResultBuilder(_state, _parser, _state.Input.CurrentLocation);
            _endSentinel = _getEndSentinel?.Invoke(builder) ?? _parser.Parse(_state);
            return _endSentinel;
        }

        if (advance)
            return _parser.Parse(_state);

        var cp = _input.Checkpoint();
        var peek = _parser.Parse(_state);
        cp.Rewind();
        return peek;
    }

    // TODO: This sequence should maintain it's own statistics, because we may be buffering or
    // generating and rewinding here.
    public SequenceStatistics GetStatistics() => _input.GetStatistics();

    public IResult<TOutput>[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (start.CompareTo(end) >= 0)
            return Array.Empty<IResult<TOutput>>();

        var currentPosition = _input.Checkpoint();
        start.Rewind();
        var buffer = new IResult<TOutput>[end.Consumed - start.Consumed];
        int i = 0;
        while (_input.Consumed < end.Consumed && i < buffer.Length)
        {
            var result = _parser.Parse(_state);
            if (!result.Success)
            {
                currentPosition.Rewind();
                return Array.Empty<IResult<TOutput>>();
            }

            if (result.Consumed == 0)
                return Array.Empty<IResult<TOutput>>();

            buffer[i++] = result;
        }

        currentPosition.Rewind();
        return buffer;
    }

    public bool Owns(SequenceCheckpoint checkpoint) => _input.Owns(checkpoint);

    public void Rewind(SequenceCheckpoint checkpoint) => _input.Rewind(checkpoint);

    public void Reset() => _input.Reset();
}
