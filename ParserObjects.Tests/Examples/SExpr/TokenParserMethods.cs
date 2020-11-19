namespace ParserObjects.Tests.Examples.SExpr
{
    using static ParserObjects.ParserMethods<Token>;

    public static class TokenParserMethods
    {
        public static IParser<Token, Token> Token(ValueType type) => Match(t => t.Type == type);
    }


}
