using System;
using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a CamelCase identifier and returns the list of individual strings in
    /// the identifier. Parses lowerCamelCase and UpperCamelCase.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> CamelCase() => _camelCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _camelCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
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
                static (u, l) => u + l
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
                static (first, rest) => new[] { first }.Concat(rest)
            );
        }
    );

    /// <summary>
    /// Parses a lowerCamelCase identifier. If the first character is a letter, it is
    /// expected to be lower-case.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> LowerCamelCase() => _lowerCamelCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _lowerCamelCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
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
                static (u, l) => u + l
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
                static (first, rest) => new[] { first }.Concat(rest)
            );
        }
    );

    /// <summary>
    /// Parses an UpperCamelCase string. If the first character is a letter, it is expected to
    /// be upper-case. Also known as 'Pascal Case'.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> UpperCamelCase() => _upperCamelCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _upperCamelCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
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
                static (u, l) => u + l
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
                static (first, rest) => new[] { first }.Concat(rest)
            );
        }
    );

    /// <summary>
    /// Matches a spinal-case identifier, with words separated by dashes. Characters can be
    /// letters of any case or digits. This is also known as 'kebab-case' or 'lisp-case'.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> SpinalCase() => _spinalCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _spinalCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
        {
            var idChar = Match(static c => char.IsLetterOrDigit(c) && c != '-');
            var word = idChar.ListCharToString();
            var separator = Match('-');
            return word.List(separator, atLeastOne: true);
        }
    );

    /// <summary>
    /// Matches a SCREAMING-SPINAL-CASE identifier, with all-upper-case words separated by
    /// dashes. Words may contain upper-case letters or digits.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> ScreamingSpinalCase() => _screamingSpinalCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _screamingSpinalCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
        {
            var idChar = Match(static c => ((char.IsLetter(c) && char.IsUpper(c)) || char.IsDigit(c)) && c != '-');
            var word = idChar.ListCharToString();
            var separator = Match('-');
            return word.List(separator, atLeastOne: true);
        }
    );

    /// <summary>
    /// Matches snake_case identifiers of letters (any case) or digits separated by
    /// underscores. Also known as pothole_case.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> SnakeCase() => _snakeCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _snakeCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
        {
            var idChar = Match(static c => char.IsLetterOrDigit(c) && c != '_');
            var word = idChar.ListCharToString();
            var separator = Match('_');
            return word.List(separator, atLeastOne: true);
        }
    );

    /// <summary>
    /// Matches SCREAMING_SNAKE_CASE with upper-case letters and digits separated by
    /// underscores. Also known as MACRO_CASE or CONSTANT_CASE.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> ScreamingSnakeCase() => _screamingSnakeCase.Value;

    private static readonly Lazy<IParser<char, IEnumerable<string>>> _screamingSnakeCase = new Lazy<IParser<char, IEnumerable<string>>>(
        static () =>
        {
            var idChar = Match(static c => ((char.IsLetter(c) && char.IsUpper(c)) || char.IsDigit(c)) && c != '_');
            var word = idChar.ListCharToString();
            var separator = Match('_');
            return word.List(separator, atLeastOne: true);
        }
    );
}
