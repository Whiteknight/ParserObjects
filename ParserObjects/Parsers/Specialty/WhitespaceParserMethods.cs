using System;
using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class WhitespaceParserMethods
    {
        /// <summary>
        /// Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc)
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> WhitespaceCharacter() => _whitespaceCharacter.Value;
        private static readonly Lazy<IParser<char, char>> _whitespaceCharacter = new Lazy<IParser<char, char>>(
            () => Match<char>(char.IsWhiteSpace).Named("ws")
        );

        /// <summary>
        /// Parses a series of whitespace characters and returns them as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Whitespace() => _whitespace.Value;
        private static readonly Lazy<IParser<char, string>> _whitespace = new Lazy<IParser<char, string>>(
            () => WhitespaceCharacter()
                .List(true)
                .Transform(w => new string(w.ToArray()))
                .Named("whitespace")
        );
    }
}