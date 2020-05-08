using System;
using System.Collections.Generic;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Specialty.DigitParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class IdentifierParserMethods
    {
        /// <summary>
        /// Parses a CamelCase identifier and returns the list of individual strings in
        /// the identifier. Parses lowerCamelCase and UpperCamelCase
        /// </summary>
        /// <returns></returns>
        public static IParser<char, IEnumerable<string>> CamelCase() => _camelCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _camelCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var lowerCase = Match<char>(char.IsLower);
                var upperCase = Match<char>(char.IsUpper);

                // Any run of digits of any length is a string
                var digits = DigitString();

                // In some cases a string of lower-cases will count
                var lowerString = lowerCase.ListCharToString(true);

                // A string starting with an upper char and continuing with zero or more lower chars
                var camelCaseString = Rule(
                    upperCase,
                    lowerCase.ListCharToString(),
                    (u, l) => u + l
                );

                // A run of all uppercase chars which aren't followed by lower-case chars
                // can be an abbreviation
                var upperString = upperCase
                    .NotFollowedBy(lowerCase)
                    .ListCharToString(true);

                var parts = First(
                    digits,
                    upperString,
                    camelCaseString,
                    lowerString
                );

                return parts.List();
            }
        );

        // TODO: Docs for these methods, when we mature this class a bit more
        // TODO: spinal-case and snake_case parsers
        // TODO: Dedicated parsers for lowerCamelCase and UpperCamelCase
    }
}