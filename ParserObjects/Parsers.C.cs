using System;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class C
    {
        /// <summary>
        /// C-style comment with '/*' ... '*/' delimiters.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment() => _comment.Value;

        private static readonly Lazy<IParser<char, string>> _comment
            = new Lazy<IParser<char, string>>(Internal.Grammars.C.CommentGrammar.CreateParser);

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
        /// C-style hexadecimal literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() => _hexString.Value;

        private static readonly Lazy<IParser<char, string>> _hexString
            = new Lazy<IParser<char, string>>(
                static () => CaptureString(HexadecimalInteger())
                    .Named("C-Style Hex String")
            );

        /// <summary>
        /// C-style hexadecimal literal returned as an unsigned integer.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, uint> HexadecimalInteger() => _hexInteger.Value;

        private static readonly Lazy<IParser<char, uint>> _hexInteger
            = new Lazy<IParser<char, uint>>(Internal.Grammars.C.Integers.CreateHexUnsignedIntegerParser);

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
        /// C-style integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> IntegerString() => _integerString.Value;

        private static readonly Lazy<IParser<char, string>> _integerString
            = new Lazy<IParser<char, string>>(
                static () =>
                {
                    return CaptureString(
                        Integer()
                    ).Named("C-Style Integer String");
                }
            );

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> Integer() => _integer.Value;

        private static readonly Lazy<IParser<char, int>> _integer
            = new Lazy<IParser<char, int>>(Internal.Grammars.C.Integers.CreateSignedIntegerParser);

        /// <summary>
        /// C-style integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> LongIntegerString() => _longIntegerString.Value;

        private static readonly Lazy<IParser<char, string>> _longIntegerString
            = new Lazy<IParser<char, string>>(
                static () =>
                {
                    return CaptureString(
                        LongInteger()
                    ).Named("C-Style Long String");
                }
            );

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, long> LongInteger() => _longInteger.Value;

        private static readonly Lazy<IParser<char, long>> _longInteger
            = new Lazy<IParser<char, long>>(Internal.Grammars.C.Integers.CreateSignedLongParser);

        /// <summary>
        /// C-style unsigned integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedIntegerString() => _unsignedIntegerString.Value;

        private static readonly Lazy<IParser<char, string>> _unsignedIntegerString
            = new Lazy<IParser<char, string>>(
                static () => CaptureString(
                        UnsignedInteger()
                    )
                    .Named("C-Style Unsigned Integer String")
            );

        /// <summary>
        /// C-style Unsigned Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, uint> UnsignedInteger() => _unsignedInteger.Value;

        private static readonly Lazy<IParser<char, uint>> _unsignedInteger
            = new Lazy<IParser<char, uint>>(Internal.Grammars.C.Integers.CreateUnsignedIntegerParser);

        /// <summary>
        /// C-style Unsigned Long Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, ulong> UnsignedLongInteger() => _unsignedLongInteger.Value;

        private static readonly Lazy<IParser<char, ulong>> _unsignedLongInteger
            = new Lazy<IParser<char, ulong>>(Internal.Grammars.C.Integers.CreateUnsignedLongParser);

        /// <summary>
        /// C-style unsigned long integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedLongIntegerString() => _unsignedLongIntegerString.Value;

        private static readonly Lazy<IParser<char, string>> _unsignedLongIntegerString
            = new Lazy<IParser<char, string>>(
                static () => CaptureString(
                        UnsignedLongInteger()
                    )
                    .Named("C-Style Unsigned Long Integer String")
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
