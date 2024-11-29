using System.Collections.Generic;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

#pragma warning disable S3251

namespace ParserObjects.Internal.Grammars.Casing;

public static class CamelCaseGrammar
{
    public static IParser<char, IEnumerable<string>> CreateParser()
    {
        var lowerCase = LowerCase();
        var upperCase = UpperCase();

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
            static (first, rest) => (IEnumerable<string>)[first, .. rest]
        );
    }

    public static IParser<char, IEnumerable<string>> CreateLowerParser()
    {
        var lowerCase = LowerCase();
        var upperCase = UpperCase();

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
            static (first, rest) => (IEnumerable<string>)[first, .. rest]
        );
    }

    public static IParser<char, IEnumerable<string>> CreateUpperParser()
    {
        var lowerCase = LowerCase();
        var upperCase = UpperCase();

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
            static (first, rest) => (IEnumerable<string>)[first, .. rest]
        );
    }
}
