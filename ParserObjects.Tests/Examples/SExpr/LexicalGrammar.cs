using static ParserObjects.CStyleParserMethods;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.SExpr
{
    public static class LexicalGrammar
    {
        public static IParser<char, Token> CreateParser()
        {
            var number = Integer().Transform(i => new Token { Type = ValueType.Number, Value = i });
            var str = String().Transform(s => new Token { Type = ValueType.QuotedString, Value = s });
            var symbol = Identifier().Transform(i => new Token { Type = ValueType.Symbol, Value = i });
            var oper = First(
                Match('.'),
                Match('+'),
                Match('-')
            ).Transform(o => new Token { Type = ValueType.Operator, Value = o.ToString() });
            var ws = Whitespace().Transform(w => new Token { Type = ValueType.Whitespace, Value = w });
            var openParen = Match('(').Transform(_ => new Token { Type = ValueType.OpenParen, Value = "(" });
            var closeParen = Match(')').Transform(_ => new Token { Type = ValueType.CloseParen, Value = ")" });

            var anyToken = First(
                ws,
                number,
                str,
                symbol,
                openParen,
                closeParen
            );

            var token = Rule(
                Produce((t, d) => t.CurrentLocation),
                anyToken,
                (location, t) =>
                {
                    t.Location = location;
                    return t;
                }
            );

            return token;
        }
    }
}
