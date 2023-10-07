using System;
using System.Collections.Generic;
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
        /// "delimited".
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier() => _identifier.Value;

        private static readonly Lazy<IParser<char, string>> _identifier
            = new Lazy<IParser<char, string>>(
                static () => IdentifierGrammar.CreateParser().Named("SQL-Style Identifier")
            );

        /// <summary>
        /// SQL-style qualified identifier which may have one or more identifiers separated by '.'.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, IReadOnlyList<string>> QualifiedIdentifier() => _qualifiedIdentifier.Value;

        private static readonly Lazy<IParser<char, IReadOnlyList<string>>> _qualifiedIdentifier
            = new Lazy<IParser<char, IReadOnlyList<string>>>(
                () => _identifier.Value.List(MatchChar('.'), 1)
            );
    }
}
