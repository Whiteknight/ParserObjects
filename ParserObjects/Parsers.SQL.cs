using System;
using System.Collections.Generic;
using ParserObjects.Internal.Grammars.Sql;
using static ParserObjects.Internal.ParserCache;

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
        public static IParser<char, string> Comment()
            => GetOrCreate("SQL-Style Comment", static () => PrefixedLine("--"));

        /// <summary>
        /// SQL-style identifier which may be T-SQL [delimited] or Oracle-style 'delimited' and
        /// "delimited".
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier()
            => GetOrCreate("SQL-Style Identifier", static () => IdentifierGrammar.CreateParser());

        /// <summary>
        /// SQL-style qualified identifier which may have one or more identifiers separated by '.'.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, IReadOnlyList<string>> QualifiedIdentifier()
            => GetOrCreate(
                "SQL-Style Qualified Identifier",
                static () => Identifier().List(MatchChar('.'), 1)
            );
    }
}
