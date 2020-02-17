namespace ParserObjects.Sequences
{
    public static class StringExtensions
    {
        public static ISequence<char> AsCharacterSequence(this string str)
            => new StringCharacterSequence(str);
    }
}