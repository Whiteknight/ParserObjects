using System;
using static ParserObjects.ParserMethods<ParserObjects.Tests.Examples.PrattCalculator.Token>;

namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public static class TokenParserExtension
    {
        public static IParser<Token, Token> Token(TokenType type)
            => Match(t => t.Type == type);

        public static IParser<Token, int> ThrowError(string message)
            => Produce<int>((t, d) => throw new Exception($"{message} at {t.CurrentLocation} ({t})"));
    }
}