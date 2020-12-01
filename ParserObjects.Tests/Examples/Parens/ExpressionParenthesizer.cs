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
                    .RightBindingPower(100)
                    .ProduceRight((ctx, value) => ctx.Parse())
                    .LeftBindingPower(100)
                    .ProduceLeft((ctx, left, value) => left.Value)
                    .Named("whitespace")
                )
                .Add(DigitString(), c => c
                    .ProduceRight((ctx, value) => value.Value)
                    .Named("number")
                )
                .Add(Match('+'), c => c
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Addition")
                )
                .Add((Match('*'), Match('/')).First(), c => c
                    .LeftBindingPower(3)
                    .ProduceLeft((ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Multiply/Divide")
                )
                .Add(Match('('), c => c
                    .LeftBindingPower(9)
                    .ProduceLeft((ctx, l, op) =>
                    {
                        var contents = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return $"({l.Value}*({contents}))";
                    })
                    .RightBindingPower(0)
                    .ProduceRight((ctx, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({op.Value}{r})";
                    })
                    .Named("Parenthesis")
                )
                .Add(Match('-'), c => c
                    .LeftBindingPower(1)
                    .ProduceRight((ctx, op) =>
                    {
                        var r = ctx.Parse(7);
                        return $"({op.Value}{r})";
                    })
                    .ProduceLeft((ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l.Value}{op.Value}{r})";
                    })
                    .Named("Subtract/Negate")
                )
                .Add(Match('!'), c => c
                    .LeftBindingPower(9)
                    .ProduceLeft((ctx, l, op) => $"({l}{op})")
                    .Named("Factorial")
                )
            );
            var result = target.Parse(expr);
            return result.Success ? result.Value : null;
        }
    }
}
