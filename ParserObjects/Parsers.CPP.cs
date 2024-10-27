using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    public static class Cpp
    {
        /// <summary>
        /// C++-style comment '//' ...
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment()
            => GetOrCreate("C++-Style Comment", static () => PrefixedLine("//"));
    }
}
