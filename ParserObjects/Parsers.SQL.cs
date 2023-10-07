using System;
using ParserObjects.Internal.Grammars.Sql;

namespace ParserObjects;

public static partial class Parsers
{
    public static class Sql
    {
        /// <summary>
        /// SQL-style comment which starts with '--' and includes all characters until the end
        /// of line.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;

        private static readonly Lazy<IParser<char, string>> _comment
            = new Lazy<IParser<char, string>>(
                static () => PrefixedLine("--").Named("SQL-Style Comment")
            );

        /// <summary>
        /// SQL-style identifier which may be T-SQL [delimited] or Oracle-style 'delimited' and
        /// "delimited"
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier() => _identifier.Value;

        private static readonly Lazy<IParser<char, string>> _identifier
            = new Lazy<IParser<char, string>>(
                static () => IdentifierGrammar.CreateParser().Named("SQL-style Identifier")
            );
    }
}
