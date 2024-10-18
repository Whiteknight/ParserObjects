using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.Casing;

public static class CamelCaseGrammar
{
    public static IParser<char, IEnumerable<string>> CreateParser()
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

    public static IParser<char, IEnumerable<string>> CreateLowerParser()
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

    public static IParser<char, IEnumerable<string>> CreateUpperParser()
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
}
