using ParserObjects.Pratt;
using static ParserObjects.Parsers<ParserObjects.Tests.Examples.PrattCalculator.Token>;
using static ParserObjects.Tests.Examples.PrattCalculator.TokenParserExtension;

namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public static class ExpressionGrammar
    {
        public static IParser<Token, int> CreateParser()
        {
            var number = Token(TokenType.Number).Transform(t => int.Parse(t.Value));

            var expression = Pratt<int>(config => config
                .Add(number)
                .Add(Token(TokenType.Addition), p => p
                    .BindLeft(1, (ctx, left, _) =>
                    {
                        var right = ctx.Parse();
                        return left.Value + right;
                    }))
                .Add(Token(TokenType.Subtraction), p => p
                    .Bind(9, (ctx, _) =>
                    {
                        var right = ctx.Parse();
                        return -right;
                    })
                    .BindLeft(1, (ctx, left, _) =>
                    {
                        var right = ctx.Parse();
                        return left.Value - right;
                    }))
                .Add(Token(TokenType.Multiplication), p => p
                    .BindLeft(3, (ctx, left, _) =>
                    {
                        var right = ctx.Parse();
                        return left.Value * right;
                    }))
                .Add(Token(TokenType.Division), p => p
                    .BindLeft(3, (ctx, left, _) =>
                    {
                        var right = ctx.Parse();
                        return left.Value / right;
                    }))
                .Add(Token(TokenType.Exponentiation), p => p
                    .BindLeft(6, 5, (ctx, left, _) =>
                    {
                        var right = ctx.Parse();
                        return (int)Math.Pow(left.Value, right);
                    }))
                .Add(Token(TokenType.Factorial), p => p
                    .BindLeft(1, 0, (_, left, _) =>
                    {
                        var product = 1;
                        for (int i = 2; i <= Math.Abs(left.Value); i++)
                            product *= i;
                        return product;
                    }))
                .Add(Token(TokenType.OpenParen), p => p
                    .Bind(0, (ctx, _) =>
                    {
                        var contents = ctx.Parse(0);
                        ctx.Expect(Token(TokenType.CloseParen));
                        return contents;
                    }))
            );

            var all = Rule(
                expression,
                Token(TokenType.End),
                (add, _) => add
            );

            return all;
        }
    }
}
