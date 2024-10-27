using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    public static class C
    {
        /// <summary>
        /// C-style comment with '/*' ... '*/' delimiters.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Comment()
            => GetOrCreate("C-Style Comment", Internal.Grammars.C.CommentGrammar.CreateParser);

        /// <summary>
        /// C-style Double literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DoubleString()
            => GetOrCreate(
                "C-Style Double String",
                static () => Capture(IntegerString(), MatchChar('.'), DigitString())
                    .Stringify()
            );

        /// <summary>
        /// C-style float/double literal returned as a parsed Double.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Double()
            => GetOrCreate(
                "C-Style Double Literal",
                static () => DoubleString().Transform(double.Parse)
            );

        /// <summary>
        /// C-style hexadecimal literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString()
            => GetOrCreate("C-Style Hex String", static () => CaptureString(HexadecimalInteger()));

        /// <summary>
        /// C-style hexadecimal literal returned as an unsigned integer.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, uint> HexadecimalInteger()
            => GetOrCreate("C-Style Hex Unsigned Integer Literal", Internal.Grammars.C.Integers.CreateHexUnsignedIntegerParser);

        /// <summary>
        /// C-style Identifier.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Identifier()
            => GetOrCreate(
                "C-Style Identifier",
                static () => Capture(
                        Match(static c => c == '_' || char.IsLetter(c)),
                        Match(static c => c == '_' || char.IsLetterOrDigit(c)).List()
                    )
                    .Stringify()
            );

        /// <summary>
        /// C-style integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> IntegerString()
            => GetOrCreate("C-Style Integer String", static () => CaptureString(Integer()));

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> Integer()
            => GetOrCreate("C-Style Signed Integer Literal", Internal.Grammars.C.Integers.CreateSignedIntegerParser);

        /// <summary>
        /// C-style integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> LongIntegerString()
            => GetOrCreate("C-Style Long String", static () => CaptureString(LongInteger()));

        /// <summary>
        /// C-style Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, long> LongInteger()
            => GetOrCreate("C-Style Signed Long Literal", Internal.Grammars.C.Integers.CreateSignedLongParser);

        /// <summary>
        /// C-style unsigned integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedIntegerString()
            => GetOrCreate("C-Style Unsigned Integer String", static () => CaptureString(UnsignedInteger()));

        /// <summary>
        /// C-style Unsigned Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, uint> UnsignedInteger()
            => GetOrCreate("C-Style Unsigned Integer Literal", Internal.Grammars.C.Integers.CreateUnsignedIntegerParser);

        /// <summary>
        /// C-style Unsigned Long Integer literal returned as a parsed Int32.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, ulong> UnsignedLongInteger()
            => GetOrCreate("C-Style Unsigned Long Literal", Internal.Grammars.C.Integers.CreateUnsignedLongParser);

        /// <summary>
        /// C-style unsigned long integer literal returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> UnsignedLongIntegerString()
            => GetOrCreate("C-Style Unsigned Long Integer String", static () => CaptureString(UnsignedLongInteger()));

        /// <summary>
        /// Parse a C-style string, removing quotes and replacing escape sequences with their
        /// proper values.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedString()
            => GetOrCreate("C-Style Stripped String", Internal.Grammars.C.StrippedStringGrammar.CreateStringParser);

        /// <summary>
        /// Parse a C-style string, keeping quotes and escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> String()
            => GetOrCreate("C-Style String", Internal.Grammars.C.StringGrammar.CreateStringParser);

        /// <summary>
        /// Parses a C-style char literal, removing quotes and resolving escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> StrippedCharacter()
            => GetOrCreate("C-Style Stripped Character", Internal.Grammars.C.StrippedStringGrammar.CreateCharacterParser);

        /// <summary>
        /// Parse a C-style char literal, keeping the quotes and escape sequences.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Character()
            => GetOrCreate("C-Style Character", Internal.Grammars.C.StringGrammar.CreateCharacterParser);
    }
}
