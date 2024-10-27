using System;
using System.Collections.Generic;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a CamelCase identifier and returns the list of individual strings in
    /// the identifier. Parses lowerCamelCase and UpperCamelCase.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> CamelCase()
        => GetOrCreate("CamelCase Words", Internal.Grammars.Casing.CamelCaseGrammar.CreateParser);

    /// <summary>
    /// Parses a lowerCamelCase identifier. If the first character is a letter, it is
    /// expected to be lower-case.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> LowerCamelCase()
        => GetOrCreate("LowerCamelCase Words", Internal.Grammars.Casing.CamelCaseGrammar.CreateLowerParser);

    /// <summary>
    /// Parses an UpperCamelCase string. If the first character is a letter, it is expected to
    /// be upper-case. Also known as 'Pascal Case'.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IEnumerable<string>> UpperCamelCase()
        => GetOrCreate("UpperCamelCase Words", Internal.Grammars.Casing.CamelCaseGrammar.CreateUpperParser);

    /// <summary>
    /// Matches a spinal-case identifier, with words separated by dashes. Characters can be
    /// letters of any case or digits. This is also known as 'kebab-case' or 'lisp-case'.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IReadOnlyList<string>> SpinalCase()
        => GetOrCreate(
            "SpinalCase Words",
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
    public static IParser<char, IReadOnlyList<string>> ScreamingSpinalCase()
        => GetOrCreate(
            "ScreamingSpinalCase Words",
            static () =>
            {
                var idChar = Match(static c => ((char.IsLetter(c) && char.IsUpper(c)) || char.IsDigit(c)) && c != '-');
                var word = idChar.ListCharToString();
                var separator = MatchChar('-');
                return word.List(separator, atLeastOne: true);
            }
        );

    /// <summary>
    /// Matches snake_case identifiers of letters (any case) or digits separated by
    /// underscores. Also known as pothole_case.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IReadOnlyList<string>> SnakeCase()
        => GetOrCreate(
            "SnakeCase Words",
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
    public static IParser<char, IReadOnlyList<string>> ScreamingSnakeCase()
        => GetOrCreate(
            "ScreamingSnakeCase Words",
            static () =>
            {
                var idChar = Match(static c => ((char.IsLetter(c) && char.IsUpper(c)) || char.IsDigit(c)) && c != '_');
                var word = idChar.ListCharToString();
                var separator = Match('_');
                return word.List(separator, atLeastOne: true);
            }
        );
}
