using System;
using static ParserObjects.ParserMethods<ParserObjects.Tests.Examples.ExprCalculator.Token>;

namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public static class TokenParserExtension
    {
        public static IParser<Token, Token> Token(TokenType type)
            => Match(t => t.Type == type);

        public static IParser<Token, int> ThrowError(string message)
            => Produce<int>(t => throw new Exception($"{message} at {t.CurrentLocation} ({t})"));
    }
}
