using System;

namespace ParserObjects;

public static partial class Parsers
{
    public static class SQL
    {
        /// <summary>
        /// SQL-style comment which starts with '--' and includes all characters until the end
        /// of line.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;

        private static readonly Lazy<IParser<char, string>> _comment = new Lazy<IParser<char, string>>(
            () => PrefixedLine("--").Named("SQL-Style Comment")
        );
    }
}
