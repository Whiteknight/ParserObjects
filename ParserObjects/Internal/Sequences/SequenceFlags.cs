namespace ParserObjects.Internal.Sequences;

public static class SequenceFlags
{
    public static SequenceStateTypes FlagsForStartOfCharSequence(bool isEnd)
    {
        var flags = SequenceStateTypes.StartOfInput | SequenceStateTypes.StartOfLine;
        if (isEnd)
            flags |= SequenceStateTypes.EndOfInput;
        return flags;
    }

    public static SequenceStateTypes FlagsForStartOfSequence(bool isEnd)
    {
        var flags = SequenceStateTypes.StartOfInput;
        if (isEnd)
            flags |= SequenceStateTypes.EndOfInput;
        return flags;
    }
}
