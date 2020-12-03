using System;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects
{
    public static partial class ParserMethods
    {
        /// <summary>
        /// Matches a Letter character.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> Letter() => _letter.Value;
        private static readonly Lazy<IParser<char, char>> _letter = new Lazy<IParser<char, char>>(
            () => Match(char.IsLetter).Named("letter")
        );

        /// <summary>
        /// Matches a series of consecutive letter characters.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Word() => _word.Value;
        private static readonly Lazy<IParser<char, string>> _word = new Lazy<IParser<char, string>>(
            () => Letter().ListCharToString(true).Named("word")
        );

        /// <summary>
        /// Matches an upper-case letter character.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> UpperCase() => _upperCase.Value;
        private static readonly Lazy<IParser<char, char>> _upperCase = new Lazy<IParser<char, char>>(
            () => Match(char.IsUpper).Named("upperCase")
        );

        /// <summary>
        /// Matches a lower-case letter character.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> LowerCase() => _lowerCase.Value;
        private static readonly Lazy<IParser<char, char>> _lowerCase = new Lazy<IParser<char, char>>(
            () => Match(char.IsLower).Named("lowerCase")
        );

        /// <summary>
        /// Matches a symbol or punctuation character.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> Symbol() => _symbol.Value;
        private static readonly Lazy<IParser<char, char>> _symbol = new Lazy<IParser<char, char>>(
            () => Match(c => char.IsPunctuation(c) || char.IsSymbol(c)).Named("lowerCase")
        );
    }
}