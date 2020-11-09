using System;
using System.Collections.Generic;
using System.Linq;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects
{
    public static partial class ParserMethods
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
                var lowerCase = Match(char.IsLower);
                var upperCase = Match(char.IsUpper);

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

                var bodyParts = First(
                    digits,
                    upperString,
                    camelCaseString,
                    lowerString
                );

                // The first part must be a character string, either upper, lower or camel
                var firstPart = First(
                    upperString,
                    lowerString,
                    camelCaseString
                );

                // A first part followed by any number of body parts
                return Rule(
                    firstPart,
                    bodyParts.List(),
                    (first, rest) => new[] { first }.Concat(rest)
                );
            }
        );

        public static IParser<char, IEnumerable<string>> LowerCamelCase() => _lowerCamelCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _lowerCamelCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var lowerCase = Match(char.IsLower);
                var upperCase = Match(char.IsUpper);

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

                var bodyParts = First(
                    digits,
                    upperString,
                    camelCaseString,
                    lowerString
                );

                return Rule(
                    lowerString,
                    bodyParts.List(),
                    (first, rest) => new[] { first }.Concat(rest)
                );
            }
        );

        public static IParser<char, IEnumerable<string>> UpperCamelCase() => _upperCamelCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _upperCamelCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var lowerCase = Match(char.IsLower);
                var upperCase = Match(char.IsUpper);

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

                var firstPart = First(
                    upperString,
                    camelCaseString
                );

                var bodyParts = First(
                    digits,
                    upperString,
                    camelCaseString,
                    lowerString
                );

                return Rule(
                    firstPart,
                    bodyParts.List(),
                    (first, rest) => new[] { first }.Concat(rest)
                );
            }
        );

        public static IParser<char, IEnumerable<string>> SpinalCase() => _spinalCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _spinalCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var idChar = Match(c => char.IsLetterOrDigit(c) && c != '-');
                var word = idChar.ListCharToString();
                var separator = Match('-');
                return word.ListSeparatedBy(separator, atLeastOne: true);
            }
        );

        public static IParser<char, IEnumerable<string>> ScreamingSpinalCase() => _screamingSpinalCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _screamingSpinalCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var idChar = Match(c => (char.IsLetter(c) && char.IsUpper(c) || char.IsDigit(c)) && c != '-');
                var word = idChar.ListCharToString();
                var separator = Match('-');
                return word.ListSeparatedBy(separator, atLeastOne: true);
            }
        );

        public static IParser<char, IEnumerable<string>> SnakeCase() => _snakeCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _snakeCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var idChar = Match(c => char.IsLetterOrDigit(c) && c != '_');
                var word = idChar.ListCharToString();
                var separator = Match('_');
                return word.ListSeparatedBy(separator, atLeastOne: true);
            }
        );

        public static IParser<char, IEnumerable<string>> ScreamingSnakeCase() => _screamingSnakeCase.Value;
        private static readonly Lazy<IParser<char, IEnumerable<string>>> _screamingSnakeCase = new Lazy<IParser<char, IEnumerable<string>>>(
            () =>
            {
                var idChar = Match(c => (char.IsLetter(c) && char.IsUpper(c) || char.IsDigit(c)) && c != '_');
                var word = idChar.ListCharToString();
                var separator = Match('_');
                return word.ListSeparatedBy(separator, atLeastOne: true);
            }
        );
    }
}