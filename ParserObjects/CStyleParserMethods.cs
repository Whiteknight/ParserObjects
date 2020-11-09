using System;
using System.Globalization;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects
{
    public static class CStyleParserMethods
    {
        /// <summary>
        /// C-style comment with '/*' ... '*/' delimiters
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;
        private static readonly Lazy<IParser<char, string>> _comment = new Lazy<IParser<char, string>>(
            () =>
            {
                var start = Match("/*").Transform(c => "/*");
                var end = Match("*/").Transform(c => "*/");
                var standaloneAsterisk = Match('*').NotFollowedBy(Match('/'));
                var notAsterisk = Match(c => c != '*');

                var bodyChar = (standaloneAsterisk, notAsterisk).First();
                var bodyChars = bodyChar.ListCharToString();

                return (start, bodyChars, end)
                    .Produce((s, b, e) => s + b + e)
                    .Named("C-Style Comment");
            }
        );

        /// <summary>
        /// C-style hexadecimal literal returned as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() => _hexString.Value;
        private static readonly Lazy<IParser<char, string>> _hexString = new Lazy<IParser<char, string>>(
            () => (Match("0x"), ParserMethods.HexadecimalString())
                .Produce((prefix, value) => prefix + value)
                .Named("C-Style Hex String")
        );

        /// <summary>
        /// C-style hexadecimal literal returned as a parsed integer
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> HexadecimalInteger() => _hexInteger.Value;

        private static readonly Lazy<IParser<char, int>> _hexInteger = new Lazy<IParser<char, int>>(
            () => (Match("0x"), ParserMethods.HexadecimalString())
                .Produce((prefix, value) => int.Parse(value, NumberStyles.HexNumber))
                .Named("C-Style Hex Literal")
        );

        /// <summary>
        /// C-style integer literal returned as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> IntegerString() => _integerString.Value;

        private static readonly Lazy<IParser<char, string>> _integerString = new Lazy<IParser<char, string>>(
            () =>
            {
                var maybeMinus = Match('-').Transform(c => "-").Optional(() => string.Empty);
                var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
                var digits = Digit().ListCharToString();
                var zero = Match('0').Transform(c => "0");
                var nonZeroNumber = (maybeMinus, nonZeroDigit, digits).Produce((sign, start, body) => sign + start + body);
                return (nonZeroNumber, zero)
                    .First()
                    .Named("C-Style Integer String");
            }
        );

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> Integer() => _integer.Value;

        private static readonly Lazy<IParser<char, int>> _integer = new Lazy<IParser<char, int>>(
            () => IntegerString()
                .Transform(int.Parse)
                .Named("C-Style Integer Literal")
        );

        /// <summary>
        /// C-style unsigned integer literal returned as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedIntegerString() => _unsignedIntegerString.Value;

        private static readonly Lazy<IParser<char, string>> _unsignedIntegerString = new Lazy<IParser<char, string>>(
            () =>
            {
                var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
                var digits = Digit().ListCharToString();
                var zero = Match('0').Transform(c => "0");
                var nonZeroNumber = (nonZeroDigit, digits).Produce((start, body) => start + body);
                return (nonZeroNumber, zero)
                    .First()
                    .Named("C-Style Unsigned Integer String");
            }
        );

        /// <summary>
        /// C-style Unsigned Integer literal returned as a parsed Int32
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> UnsignedInteger() => _unsignedInteger.Value;

        private static readonly Lazy<IParser<char, int>> _unsignedInteger = new Lazy<IParser<char, int>>(
            () => UnsignedIntegerString()
                .Transform(int.Parse)
                .Named("C-Style Unsigned Integer Literal")
        );

        /// <summary>
        /// C-style Double literal returned as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DoubleString() => _doubleString.Value;

        private static readonly Lazy<IParser<char, string>> _doubleString = new Lazy<IParser<char, string>>(
            () => (IntegerString(), Match('.'), DigitString())
                .Produce((whole, dot, fract) => whole + dot + fract)
                .Named("C-Style Double String"));

        /// <summary>
        /// C-style float/double literal returned as a parsed Double
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Double() => _double.Value;

        private static readonly Lazy<IParser<char, double>> _double = new Lazy<IParser<char, double>>(
            () => DoubleString()
                .Transform(double.Parse)
                .Named("C-Style Double Literal")
        );

        /// <summary>
        /// C-style Identifier
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier() => _identifier.Value;

        private static readonly Lazy<IParser<char, string>> _identifier = new Lazy<IParser<char, string>>(
            () =>
            {
                var startChar = Match(c => c == '_' || char.IsLetter(c));
                var bodyChar = Match(c => c == '_' || char.IsLetterOrDigit(c));
                var bodyChars = bodyChar.ListCharToString();
                return (startChar, bodyChars)
                    .Produce((start, rest) => start + rest)
                    .Named("C-Style Identifier");
            }
        );
    }
}