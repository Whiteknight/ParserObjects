using System;

namespace ParserObjects;

public static partial class Parsers
{
    public static class CPP
    {
        /// <summary>
        /// C++-style comment '//' ...
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;

        private static readonly Lazy<IParser<char, string>> _comment = new Lazy<IParser<char, string>>(
            static () => PrefixedLine("//").Named("C++-Style Comment")
        );
    }
}
