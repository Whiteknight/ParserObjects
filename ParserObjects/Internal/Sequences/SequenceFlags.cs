namespace ParserObjects.Internal.Sequences;

public static class SequenceFlags
{
    public static SequencePositionFlags FlagsForStartOfCharSequence(bool isEnd)
    {
        var flags = SequencePositionFlags.StartOfInput | SequencePositionFlags.StartOfLine;
        if (isEnd)
            flags |= SequencePositionFlags.EndOfInput;
        return flags;
    }

    public static SequencePositionFlags FlagsForStartOfSequence(bool isEnd)
    {
        var flags = SequencePositionFlags.StartOfInput;
        if (isEnd)
            flags |= SequencePositionFlags.EndOfInput;
        return flags;
    }
}
