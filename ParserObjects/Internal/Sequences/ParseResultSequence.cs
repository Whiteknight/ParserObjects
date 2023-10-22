using System;

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
    private readonly Func<ResultFactory<TInput, TOutput>, IResult<TOutput>>? _getEndSentinel;

    private WorkingSequenceStatistics _stats;
    private IResult<TOutput>? _endSentinel;

    public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Func<ResultFactory<TInput, TOutput>, IResult<TOutput>>? getEndSentinel, Action<string> log)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotNull(parser);
        _state = new ParseState<TInput>(input, log);
        _input = input;
        _parser = parser;
        _getEndSentinel = getEndSentinel;
        _endSentinel = null;
        _stats = default;
    }

    public IResult<TOutput> GetNext() => GetNext(true);

    public IResult<TOutput> Peek() => GetNext(false);

    public Location CurrentLocation => _input.CurrentLocation;

    public bool IsAtEnd => _input.IsAtEnd;

    public int Consumed => _input.Consumed;

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return _input.Checkpoint();
    }

    private IResult<TOutput> GetNext(bool advance)
    {
        if (_input.IsAtEnd)
        {
            if (_endSentinel != null)
                return _endSentinel;

            var builder = new ResultFactory<TInput, TOutput>(_parser, _state, default);
            _endSentinel = _getEndSentinel?.Invoke(builder) ?? _parser.Parse(_state);
            return _endSentinel;
        }

        if (advance)
        {
            var result = _parser.Parse(_state);
            _stats.ItemsRead++;
            return result;
        }

        var cp = _input.Checkpoint();
        var peek = _parser.Parse(_state);
        cp.Rewind();
        _stats.ItemsPeeked++;
        return peek;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

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
            _stats.ItemsRead++;
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

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        _input.Rewind(checkpoint);
    }

    public void Reset()
    {
        _stats.Rewinds++;
        _input.Reset();
    }
}
