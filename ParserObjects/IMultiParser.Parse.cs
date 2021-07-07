using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class MultiParserExtensions
    {
        public static IMultiResult<TOutput> Parse<TOutput>(this IMultiParser<char, TOutput> p, string s)
            => p.Parse(new ParseState<char>(new StringCharacterSequence(s), Defaults.LogMethod));
    }
}
