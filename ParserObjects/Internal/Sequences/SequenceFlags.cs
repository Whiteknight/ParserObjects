namespace ParserObjects.Internal.Sequences;

public static class SequenceFlags
{
    public static SequenceStateType FlagsForStartOfCharSequence(bool isEnd)
    {
        var flags = SequenceStateType.StartOfInput | SequenceStateType.StartOfLine;
        if (isEnd)
            flags |= SequenceStateType.EndOfInput;
        return flags;
    }

    public static SequenceStateType FlagsForStartOfSequence(bool isEnd)
    {
        var flags = SequenceStateType.StartOfInput;
        if (isEnd)
            flags |= SequenceStateType.EndOfInput;
        return flags;
    }
}
