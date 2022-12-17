using System;
using System.Diagnostics;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of characters read from a string. Does not do any normalization of line endings.
/// </summary>
public sealed class NonnormalizedStringCharacterSequence : ICharSequenceWithRemainder
{
    private readonly string _s;
    private readonly SequenceOptions<char> _options;

    private WorkingSequenceStatistics _stats;
    private int _index;
    private int _line;
    private int _column;

    public NonnormalizedStringCharacterSequence(string s, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(s, nameof(s));
        _options = options;
        _options.Validate();
        Debug.Assert(_options.MaintainLineEndings, "Only used when line-ending normalization is off");
        _stats = default;
        _s = s;
        _line = 1;
        _column = 0;
    }

    public NonnormalizedStringCharacterSequence(string s)
        : this(s, default)
    {
    }

    public char GetNext()
    {
        var next = GetNextInternal(true);
        if (next == _options.EndSentinel)
            return next;

        _stats.ItemsRead++;

        // If we have a newline, update the line-tracking.
        if (next == '\n')
        {
            _line++;
            _column = 0;
            return next;
        }

        // Bump counts and return
        _column++;
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
        _stats.ItemsPeeked++;
        return next;
    }

    public Location CurrentLocation => new Location(_options.FileName, _line, _column);

    public bool IsAtEnd => _index >= _s.Length;

    public int Consumed => _index;

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
    }

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _index, _index, 0L, new Location(_options.FileName, _line, _column));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        _stats.RewindsToCurrentBuffer++;
        _index = checkpoint.Index;
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public char[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (!Owns(start) || !Owns(end))
            return Array.Empty<char>();

        if (start.CompareTo(end) >= 0)
            return Array.Empty<char>();

        return _s.Substring(start.Consumed, end.Consumed - start.Consumed).ToCharArray();
    }

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
}
