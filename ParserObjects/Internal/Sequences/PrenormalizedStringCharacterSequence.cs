using System;
using System.Diagnostics;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of characters read from a string. Pre-normalizes the string to optimize for reads.
/// Only useful when normalization is requested (MaintainLineEndings == false).
/// </summary>
public sealed class PrenormalizedStringCharacterSequence : ICharSequence
{
    private readonly char[] _s;
    private readonly int _bufferLength;
    private readonly SequenceOptions<char> _options;

    private WorkingSequenceStatistics _stats;
    private int _index;
    private int _line;
    private int _column;

    public PrenormalizedStringCharacterSequence(string s, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(s, nameof(s));
        _options = options;
        _options.Validate();
        Debug.Assert(_options.NormalizeLineEndings, "This sequence requires strings be normalized");
        _stats = default;
        (_s, _bufferLength) = Normalize(s);
        _line = 1;
        _column = 0;
        _index = 0;
    }

    public PrenormalizedStringCharacterSequence(string s)
        : this(s, default)
    {
    }

    private static (char[] buffer, int length) Normalize(string s)
    {
        if (s.Length == 0)
            return (Array.Empty<char>(), 0);
        if (s.Length == 1)
            return (new[] { s[0] == '\r' ? '\n' : s[0] }, 1);

        var chars = new char[s.Length];

        int destIdx = 0;
        int srcIdx = 0;
        for (; srcIdx < s.Length - 1; srcIdx++)
        {
            // \r -> \n
            // \r\n -> \n
            var c = s[srcIdx];
            if (c != '\r')
            {
                chars[destIdx++] = c;
                continue;
            }

            var next = s[srcIdx + 1];
            if (next == '\n')
            {
                chars[destIdx++] = '\n';
                srcIdx++;
                continue;
            }

            chars[destIdx++] = '\n';
        }

        if (srcIdx < s.Length)
        {
            var c = s[srcIdx];
            if (c == '\r')
                chars[destIdx++] = '\n';
            else
                chars[destIdx++] = c;
        }

        return (chars, destIdx);
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
            return new string(_s);
        if (_index >= _s.Length)
            return string.Empty;
        return new string(_s, _index, _bufferLength - _index);
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
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return Array.Empty<char>();

        var size = end.Consumed - start.Consumed;
#if NET5_0_OR_GREATER
        return new ArraySegment<char>(_s, start.Consumed, size).ToArray();
#else
        var c = new char[size];
        for (int i = 0; i < size; i++)
            c[i] = _s[start.Consumed + i];
        return c;
#endif
    }

    public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        => new string(GetBetween(start, end));

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
}
