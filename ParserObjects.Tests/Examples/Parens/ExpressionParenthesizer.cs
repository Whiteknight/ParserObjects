using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.Parens
{
    public static class ExpressionParenthesizer
    {
        public static string Parenthesize(string expr)
        {
            var target = Pratt<string>(config => config
                .Add(Whitespace(), c => c
                    .Bind(100, (ctx, value) => ctx.Parse())
                    .BindLeft(100, (ctx, left, value) => left.Value)
                    .Named("whitespace")
                )
                .Add(DigitString(), c => c
                    .Bind(0, (ctx, value) => value.Value)
                    .Named("number")
                )
                .Add(Match('+'), c => c
                    .BindLeft(1, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Addition")
                )
                .Add((Match('*'), Match('/')).First(), c => c
                    .BindLeft(3, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Multiply/Divide")
                )
                .Add(Match('('), c => c
                    .BindLeft(9, (ctx, l, op) =>
                    {
                        var contents = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return $"({l.Value}*({contents}))";
                    })
                    .Bind(0, (ctx, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({op.Value}{r})";
                    })
                    .Named("Parenthesis")
                )
                .Add(Match('-'), c => c
                    .Bind(1, (ctx, op) =>
                    {
                        var r = ctx.Parse(7);
                        return $"({op.Value}{r})";
                    })
                    .BindLeft(1, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Subtract/Negate")
                )
                .Add(Match('!'), c => c
                    .BindLeft(9, (ctx, l, op) => $"({l}{op})")
                    .Named("Factorial")
                )
            );
            var result = target.Parse(expr);
            return result.Success ? result.Value : null;
        }
    }
}
