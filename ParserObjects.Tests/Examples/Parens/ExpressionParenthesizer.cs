using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.Parens
{
    public static class ExpressionParenthesizer
    {
        public static string Parenthesize(string expr)
        {
            var number = DigitString();
            var target = Pratt<char, string>(number, config => config
                .AddInfixOperator((Match('+'), Match('-')).First(), 1, 2, (l, op, r) => $"({l}{op}{r})")
                .AddInfixOperator((Match('*'), Match('/')).First(), 3, 4, (l, op, r) => $"({l}{op}{r})")
                .AddPostcircumfixOperator(Match('('), Match(')'), 5, (left, open, contents, close) => $"({left}*({contents}))")
                .AddPrefixOperator(Match('-'), 7, (op, r) => $"({op}{r})")
                .AddPostfixOperator(Match('!'), 9, (op, r) => $"({op}{r})")
                .AddCircumfixOperator(Match('('), Match(')'), (open, contents, close) => $"({contents})")
                
            );
            var result = target.Parse(expr);
            return result.Success ? result.Value : null;
        }
    }
}
