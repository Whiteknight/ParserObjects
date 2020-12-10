using ParserObjects.Sequences;

namespace ParserObjects
{
    public static class StringExtensions
    {
        /// <summary>
        /// Wrap the string as a sequence of characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharacterSequence(this string str)
            => new StringCharacterSequence(str);
    }
}
