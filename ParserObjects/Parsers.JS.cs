using System;
using System.Collections.Generic;
using System.Globalization;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class JS
    {
        /// <summary>
        /// JavaScript-style number literal, returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> NumberString() => _numberString.Value;

        private static readonly Lazy<IParser<char, string>> _numberString = new Lazy<IParser<char, string>>(
            static () =>
            {
                var maybeMinus = MatchChar('-').Transform(_ => "-").Optional(() => "");
                var zero = MatchChar('0').Transform(_ => "0");
                var maybeDigits = Digit().ListCharToString();
                var empty = Produce(() => "");

                // wholePart := '0' | <nonZeroDigit> <digits>*
                var wholePart = First(
                        zero,
                        Rule(
                            NonZeroDigit(),
                            maybeDigits,
                            (first, rest) => first + rest
                        )
                    );

                // fractPart := '.' <digit>+ | <empty>
                var fractPart = First(
                        Rule(
                            MatchChar('.').Transform(_ => "."),
                            DigitString(),
                            (dot, fract) => dot + fract
                        ),
                        empty
                    );

                // expExpr := ('e' | 'E') ('+' | '-' | <empty>) <digit>+
                var expExpr = Rule(
                        First(
                            MatchChar('e').Transform(_ => "e"),
                            MatchChar('E').Transform(_ => "E")
                        ),
                        First(
                            MatchChar('+').Transform(_ => "+"),
                            MatchChar('-').Transform(_ => "-"),
                            Produce(() => "+")
                        ),
                        DigitString(),
                        (e, sign, value) => e + sign + value
                    );

                // expPart := <exprExpr> | <empty>
                var expPart = First(
                        expExpr,
                        empty
                    );

                // number := <minus>? <wholePart> <fractPart> <expPart>
                return (maybeMinus, wholePart, fractPart, expPart)
                        .Rule(static (sign, whole, fract, exp) => sign + whole + fract + exp)
                        .Named("JavaScript-Style Number String");
            }
        );

        /// <summary>
        /// JavaScript-style number literal returned as a parsed Double.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Number() => _number.Value;

        private static readonly Lazy<IParser<char, double>> _number = new Lazy<IParser<char, double>>(
            () => NumberString()
                .Transform(static s => double.Parse(s, NumberStyles.Float))
                .Named("JavaScript-Style Number Literal")
        );

        private static readonly Dictionary<char, string> _escapableStringChars = new Dictionary<char, string>
        {
            { 'b', "\b" },
            { 'f', "\f" },
            { 'n', "\n" },
            { 'r', "\r" },
            { 't', "\t" },
            { 'v', "\v" },
            { '0', "\0" },
            { '\\', "\\" }
        };

        /// <summary>
        /// Parse a JavaScript-style string, removing quotes and replacing escape sequences
        /// with their literal values.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedString() => _strippedString.Value;

        private static readonly Lazy<IParser<char, string>> _strippedString = new Lazy<IParser<char, string>>(
            () =>
            {
                var escapeCharacter = Match(c => _escapableStringChars.ContainsKey(c)).Transform(c => _escapableStringChars[c]);

                var hexSequence = Rule(
                    MatchChar('x'),
                    HexadecimalDigit().ListCharToString(2, 2),
                    static (_, hex) => ((char)int.Parse(hex, NumberStyles.HexNumber)).ToString()
                );

                var unicodeEscapeSequence = Rule(
                    MatchChar('u'),
                    HexadecimalDigit().ListCharToString(4, 4),
                    static (_, hex) => char.ConvertFromUtf32(int.Parse(hex, NumberStyles.HexNumber))
                );

                var unicodeCodePointEscapeSequence = Rule(
                    Match("u{"),
                    HexadecimalDigit().ListCharToString(1, 8),
                    MatchChar('}'),
                    static (_, hex, _) => char.ConvertFromUtf32(int.Parse(hex, NumberStyles.HexNumber))
                );

                var escapeSequenceForSingleQuotedString = Rule(
                    MatchChar('\\'),
                    First(
                        MatchChar('\n').Transform(_ => ""),
                        MatchChar('\'').Transform(_ => "'"),
                        escapeCharacter,
                        hexSequence,
                        unicodeEscapeSequence,
                        unicodeCodePointEscapeSequence
                    ),
                    static (_, escape) => escape
                );

                var bodyCharForSingleQuotedString = First(
                    escapeSequenceForSingleQuotedString,
                    Match(static c => c != '\\' && c != '\'').Transform(c => c.ToString())
                );

                var singleQuotedString = Rule(
                    MatchChar('\''),
                    bodyCharForSingleQuotedString.ListStringsToString(),
                    MatchChar('\''),
                    static (_, body, _) => body
                ).Named("JavaScript-Style Single-Quoted Stripped String");

                var escapeSequenceForDoubleQuotedString = Rule(
                    MatchChar('\\'),
                    First(
                        MatchChar('\n').Transform(_ => ""),
                        MatchChar('"').Transform(_ => "\""),
                        escapeCharacter,
                        hexSequence,
                        unicodeEscapeSequence,
                        unicodeCodePointEscapeSequence
                    ),
                    static (_, escape) => escape
                );

                var bodyCharForDoubleQuotedString = First(
                    escapeSequenceForDoubleQuotedString,
                    Match(c => c != '\\' && c != '"').Transform(c => c.ToString())
                );

                var doubleQuotedString = Rule(
                    MatchChar('"'),
                    bodyCharForDoubleQuotedString.ListStringsToString(),
                    MatchChar('"'),
                    static (_, body, _) => body
                ).Named("JavaScript-Style Double-Quoted Stripped String");

                return First(
                    doubleQuotedString,
                    singleQuotedString
                ).Named("JavaScript-Style Stripped String");
            }
        );

        /// <summary>
        /// Parse a JavaScript-style string, returning the complete string literal with quotes
        /// and escape sequences unmodified.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> String() => _string.Value;

        private static readonly Lazy<IParser<char, string>> _string = new Lazy<IParser<char, string>>(
            () =>
            {
                var escapeCharacter = Match(static c => _escapableStringChars.ContainsKey(c)).Transform(c => c.ToString());

                var hexSequence = Rule(
                    MatchChar('x'),
                    HexadecimalDigit().ListCharToString(2, 2),
                    static (_, hex) => "x" + hex
                );

                var unicodeEscapeSequence = Rule(
                    MatchChar('u'),
                    HexadecimalDigit().ListCharToString(4, 4),
                    static (_, hex) => "u" + hex
                );

                var unicodeCodePointEscapeSequence = Rule(
                    Match("u{"),
                    HexadecimalDigit().ListCharToString(1, 8),
                    MatchChar('}'),
                    static (_, hex, _) => "u{" + hex + "}"
                );

                var escapeSequenceForSingleQuotedString = Rule(
                    MatchChar('\\'),
                    First(
                        MatchChar('\n').Transform(_ => ""),
                        MatchChar('\'').Transform(_ => "'"),
                        escapeCharacter,
                        hexSequence,
                        unicodeEscapeSequence,
                        unicodeCodePointEscapeSequence
                    ),
                    static (_, escape) => "\\" + escape
                );

                var bodyCharForSingleQuotedString = First(
                    escapeSequenceForSingleQuotedString,
                    Match(static c => c != '\\' && c != '\'').Transform(c => c.ToString())
                );

                var singleQuotedString = Rule(
                    MatchChar('\''),
                    bodyCharForSingleQuotedString.ListStringsToString(),
                    MatchChar('\''),
                    static (_, body, _) => "'" + body + "'"
                ).Named("JavaScript-Style Single-Quoted String");

                var escapeSequenceForDoubleQuotedString = Rule(
                    MatchChar('\\'),
                    First(
                        MatchChar('\n').Transform(_ => ""),
                        MatchChar('"').Transform(_ => "\""),
                        escapeCharacter,
                        hexSequence,
                        unicodeEscapeSequence,
                        unicodeCodePointEscapeSequence
                    ),
                    static (_, escape) => "\\" + escape
                );

                var bodyCharForDoubleQuotedString = First(
                    escapeSequenceForDoubleQuotedString,
                    Match(static c => c != '\\' && c != '"').Transform(c => c.ToString())
                );

                var doubleQuotedString = Rule(
                    MatchChar('"'),
                    bodyCharForDoubleQuotedString.ListStringsToString(),
                    MatchChar('"'),
                    static (_, body, _) => "\"" + body + "\""
                ).Named("JavaScript-Style Double-Quoted String");

                return First(
                    doubleQuotedString,
                    singleQuotedString
                ).Named("JavaScript-Style String");
            }
        );
    }
}
