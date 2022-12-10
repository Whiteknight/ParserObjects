using System;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of characters read from a string.
/// </summary>
public sealed class StringCharacterSequence : ICharSequenceWithRemainder
{
    private readonly string _s;
    private readonly SequenceOptions<char> _options;

    private WorkingSequenceStatistics _stats;
    private int _index;
    private int _line;
    private int _column;
    private int _consumed;

    public StringCharacterSequence(string s, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(s, nameof(s));
        _stats = default;
        _s = s;
        _line = 1;
        _column = 0;
        _options = options;
        _options.Validate();
        _consumed = 0;
    }

    public StringCharacterSequence(string s)
        : this(s, default)
    {
    }

    public char GetNext()
    {
        var next = GetNextInternal(true);
        if (next == _options.EndSentinel)
            return next;

        // If line endings are normalized, we replace \r -> \n and \r\n -> \n
        if (_options.NormalizeLineEndings && next == '\r')
        {
            if (GetNextInternal(false) == '\n')
                GetNextInternal(true);
            next = '\n';
        }

        _stats.ItemsRead++;

        // If we have a newline, update the line-tracking.
        if (next == '\n')
        {
            _line++;
            _column = 0;
            _consumed++;
            return next;
        }

        // Bump counts and return
        _column++;
        _consumed++;
        return next;
    }

    private char GetNextInternal(bool advance)
    {
        if (_index >= _s.Length)
            return _options.EndSentinel;
        var value = _s[_index];
        if (advance)
            _index++;
        return value;
    }

    public char Peek()
    {
        var next = GetNextInternal(false);
        if (_options.NormalizeLineEndings && next == '\r')
            next = '\n';
        _stats.ItemsPeeked++;
        return next;
    }

    public Location CurrentLocation => new Location(_options.FileName, _line, _column);

    public bool IsAtEnd => _index >= _s.Length;

    public int Consumed => _consumed;

    public string GetRemainder()
    {
        if (_index == 0)
            return _s;
        if (_index >= _s.Length)
            return string.Empty;
        return _s.Substring(_index);
    }

    public void Reset()
    {
        _index = 0;
        _line = 0;
        _column = 0;
        _consumed = 0;
    }

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _consumed, _index, 0L, new Location(_options.FileName, _line, _column));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        _stats.RewindsToCurrentBuffer++;
        _index = checkpoint.Index;
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
        _consumed = checkpoint.Consumed;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public char[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (!Owns(start) || !Owns(end))
            return Array.Empty<char>();

        if (start.CompareTo(end) >= 0)
            return Array.Empty<char>();

        var currentPosition = Checkpoint();
        start.Rewind();
        var buffer = new char[end.Consumed - start.Consumed];
        for (int i = 0; i < end.Consumed - start.Consumed; i++)
            buffer[i] = GetNext();
        currentPosition.Rewind();
        return buffer;
    }

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
}
