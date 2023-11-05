using System;
using System.Globalization;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class C
    {
        private static readonly IParser<char, string> _hexPrefix = MatchChars("0x");

        /// <summary>
        /// C-style comment with '/*' ... '*/' delimiters.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;

        private static readonly Lazy<IParser<char, string>> _comment
            = new Lazy<IParser<char, string>>(Internal.Grammars.C.CommentGrammar.CreateParser);

        /// <summary>
        /// C-style hexadecimal literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() => _hexString.Value;

        private static readonly Lazy<IParser<char, string>> _hexString
            = new Lazy<IParser<char, string>>(
                static () => (_hexPrefix, HexadecimalDigit().ListCharToString(1, 8))
                    .Rule(static (_, value) => "0x" + value)
                    .Named("C-Style Hex String")
            );

        /// <summary>
        /// C-style hexadecimal literal returned as a parsed integer.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> HexadecimalInteger() => _hexInteger.Value;

        private static readonly Lazy<IParser<char, int>> _hexInteger
            = new Lazy<IParser<char, int>>(
                static () => (_hexPrefix, HexadecimalDigit().ListCharToString(1, 8))
                    .Rule(static (_, value) => int.Parse(value, NumberStyles.HexNumber))
                    .Named("C-Style Hex Literal")
            );

        /// <summary>
        /// C-style integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> IntegerString() => _integerString.Value;

        private static readonly Lazy<IParser<char, string>> _integerString
            = new Lazy<IParser<char, string>>(
                static () =>
                {
                    var nonZeroDigit = Match(static c => char.IsDigit(c) && c != '0');
                    var digits = Digit().List();
                    var parser = First(
                        Capture(
                            MatchChar('-').Optional(),
                            nonZeroDigit,
                            digits
                        ).Stringify(),
                        MatchChar('0').Transform(static _ => "0")
                    );
                    return parser.Named("C-Style Integer String");
                }
            );

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> Integer() => _integer.Value;

        private static readonly Lazy<IParser<char, int>> _integer
            = new Lazy<IParser<char, int>>(
                static () => IntegerString()
                    .Transform(int.Parse)
                    .Named("C-Style Integer Literal")
            );

        /// <summary>
        /// C-style unsigned integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedIntegerString() => _unsignedIntegerString.Value;

        private static readonly Lazy<IParser<char, string>> _unsignedIntegerString
            = new Lazy<IParser<char, string>>(
                static () =>
                {
                    var nonZeroDigit = Match(static c => char.IsDigit(c) && c != '0');
                    var digits = Digit().List();
                    var parser = First(
                        Capture(
                            MatchChar('-').Optional(),
                            nonZeroDigit,
                            digits
                        ).Stringify(),
                        MatchChar('0').Transform(static _ => "0")
                    );
                    return parser.Named("C-Style Unsigned Integer String");
                }
            );

        /// <summary>
        /// C-style Unsigned Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> UnsignedInteger() => _unsignedInteger.Value;

        private static readonly Lazy<IParser<char, int>> _unsignedInteger
            = new Lazy<IParser<char, int>>(
                static () => UnsignedIntegerString()
                    .Transform(int.Parse)
                    .Named("C-Style Unsigned Integer Literal")
            );

        /// <summary>
        /// C-style Double literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DoubleString() => _doubleString.Value;

        private static readonly Lazy<IParser<char, string>> _doubleString
            = new Lazy<IParser<char, string>>(
                static () => Capture(IntegerString(), MatchChar('.'), DigitString())
                    .Stringify()
                    .Named("C-Style Double String")
            );

        /// <summary>
        /// C-style float/double literal returned as a parsed Double.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Double() => _double.Value;

        private static readonly Lazy<IParser<char, double>> _double
            = new Lazy<IParser<char, double>>(
                static () => DoubleString()
                    .Transform(double.Parse)
                    .Named("C-Style Double Literal")
            );

        /// <summary>
        /// C-style Identifier.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier() => _identifier.Value;

        private static readonly Lazy<IParser<char, string>> _identifier
            = new Lazy<IParser<char, string>>(
                static () =>
                {
                    var startChar = Match(static c => c == '_' || char.IsLetter(c));
                    var bodyChar = Match(static c => c == '_' || char.IsLetterOrDigit(c));
                    var parser = Capture(
                        startChar,
                        bodyChar.List()
                    );
                    return parser
                        .Stringify()
                        .Named("C-Style Identifier");
                }
            );

        /// <summary>
        /// Parse a C-style string, removing quotes and replacing escape sequences with their
        /// proper values.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedString() => _strippedString.Value;

        private static readonly Lazy<IParser<char, string>> _strippedString
            = new Lazy<IParser<char, string>>(Internal.Grammars.C.StrippedStringGrammar.CreateStringParser);

        /// <summary>
        /// Parse a C-style string, keeping quotes and escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> String() => _string.Value;

        private static readonly Lazy<IParser<char, string>> _string
            = new Lazy<IParser<char, string>>(Internal.Grammars.C.StringGrammar.CreateStringParser);

        /// <summary>
        /// Parses a C-style char literal, removing quotes and resolving escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> StrippedCharacter() => _strippedCharacter.Value;

        private static readonly Lazy<IParser<char, char>> _strippedCharacter
            = new Lazy<IParser<char, char>>(Internal.Grammars.C.StrippedStringGrammar.CreateCharacterParser);

        /// <summary>
        /// Parse a C-style char literal, keeping the quotes and escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Character() => _character.Value;

        private static readonly Lazy<IParser<char, string>> _character
            = new Lazy<IParser<char, string>>(Internal.Grammars.C.StringGrammar.CreateCharacterParser);
    }
}
