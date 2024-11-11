using System;
using System.Buffers;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// An adaptor to change output values from an IParser into an ISequence of results.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class ParseResultSequence<TInput, TOutput> : ISequence<Result<TOutput>>
{
    private readonly ISequence<TInput> _input;
    private readonly IParseState<TInput> _state;
    private readonly IParser<TInput, TOutput> _parser;
    private readonly Func<ResultFactory<TInput, TOutput>, Result<TOutput>>? _getEndSentinel;

    private WorkingSequenceStatistics _stats;
    private Result<TOutput> _endSentinel;

    public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Func<ResultFactory<TInput, TOutput>, Result<TOutput>>? getEndSentinel, Action<string> log)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotNull(parser);
        _state = new ParseState<TInput>(input, log);
        _input = input;
        _parser = parser;
        _getEndSentinel = getEndSentinel;
        _endSentinel = default;
        _stats = default;
    }

    public Result<TOutput> GetNext() => GetNext(true);

    public Result<TOutput> Peek() => GetNext(false);

    public Location CurrentLocation => _input.CurrentLocation;

    public bool IsAtEnd => _input.IsAtEnd;

    public SequenceStateTypes Flags => _input.Flags;

    public int Consumed => _input.Consumed;

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return _input.Checkpoint();
    }

    private Result<TOutput> GetNext(bool advance)
    {
        if (_input.IsAtEnd)
        {
            if (_endSentinel != default)
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

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<Result<TOutput>, TData, TResult> map)
    {
        if (start.CompareTo(end) >= 0)
            return map(ReadOnlySpan<Result<TOutput>>.Empty, data);

        var currentPosition = _input.Checkpoint();
        start.Rewind();
        var size = end.Consumed - start.Consumed;

        var buffer = ArrayPool<Result<TOutput>>.Shared.Rent(size);

        int i = 0;
        while (_input.Consumed < end.Consumed && i < buffer.Length)
        {
            var item = _parser.Parse(_state);
            _stats.ItemsRead++;
            if (!item.Success)
            {
                currentPosition.Rewind();
                return map(ReadOnlySpan<Result<TOutput>>.Empty, data);
            }

            if (item.Consumed == 0)
                return map(ReadOnlySpan<Result<TOutput>>.Empty, data);

            buffer[i++] = item;
        }

        currentPosition.Rewind();
        var result = map(buffer.AsSpan(0, size), data);
        ArrayPool<Result<TOutput>>.Shared.Return(buffer);
        return result;
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
