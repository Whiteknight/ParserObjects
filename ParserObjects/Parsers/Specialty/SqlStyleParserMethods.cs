using System;
using static ParserObjects.Parsers.Specialty.LineParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class SqlStyleParserMethods
    {
        /// <summary>
        /// SQL-style comment --....
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;
        private static readonly Lazy<IParser<char, string>> _comment = new Lazy<IParser<char, string>>(
            () => PrefixedLine("--").Named("SQL-Style Comment")
        );
    }
}