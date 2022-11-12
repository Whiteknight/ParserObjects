using ParserObjects.Utility;

namespace ParserObjects.Sequences;

/// <summary>
/// A sequence of characters read from a string.
/// </summary>
public sealed class StringCharacterSequence : ICharSequenceWithRemainder, ISequence<char>
{
    private readonly string _s;
    private readonly Options _options;

    private SequenceStatistics _stats;
    private int _index;
    private int _line;
    private int _column;
    private int _consumed;

    public record struct Options(string FileName = "", bool NormalizeLineEndings = true, char EndSentinel = '\0')
    {
        public void Validate()
        {
            if (FileName == null)
                FileName = "";
        }
    }

    public StringCharacterSequence(string s, Options options)
    {
        Assert.ArgumentNotNull(s, nameof(s));
        options.Validate();
        _stats = default;
        _s = s;
        _line = 1;
        _column = 0;
        _options = options;
        _consumed = 0;
    }

    public StringCharacterSequence(string s)
        : this(s, new Options("", true, '\0'))
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

    public ISequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new StringCheckpoint(this, _index, _line, _column, _consumed);
    }

    private record StringCheckpoint(StringCharacterSequence S, int Index, int Line, int Column, int Consumed)
        : ISequenceCheckpoint
    {
        public Location Location => new Location(S._options.FileName, Line, Column);

        public void Rewind() => S.Rewind(Index, Line, Column, Consumed);
    }

    private void Rewind(int index, int line, int column, int consumed)
    {
        _stats.Rewinds++;
        _stats.RewindsToCurrentBuffer++;
        _index = index;
        _line = line;
        _column = column;
        _consumed = consumed;
    }

    public ISequenceStatistics GetStatistics() => _stats;
}
