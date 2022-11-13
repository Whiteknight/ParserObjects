using static ParserObjects.Parsers<ParserObjects.Tests.Examples.SExpr.Token>;

namespace ParserObjects.Tests.Examples.SExpr
{
    public static class TokenParserMethods
    {
        public static IParser<Token, Token> Token(ValueType type) => Match(t => t.Type == type);
    }
}
