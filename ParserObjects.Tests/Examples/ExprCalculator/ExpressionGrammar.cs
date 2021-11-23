using ParserObjects.Utility;
using static ParserObjects.ParserMethods<ParserObjects.Tests.Examples.ExprCalculator.Token>;
using static ParserObjects.Tests.Examples.ExprCalculator.TokenParserExtension;

namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public static class ExpressionGrammar
    {
        public static IParser<Token, int> CreateParser()
        {
            var number = Token(TokenType.Number).Transform(t => int.Parse(t.Value));

            var requiredNumber = First(
                number,
                ThrowError("Expected number")
            );

            var multiplicative = LeftApply(
                number,
                left => First(
                    Rule(
                        left,
                        Token(TokenType.Multiplication),
                        requiredNumber,
                        (l, op, r) => l * r
                    ),
                    Rule(
                        left,
                        Token(TokenType.Division),
                        requiredNumber,
                        (l, op, r) => l / r
                    )
                )
            );

            var requiredMultiplicative = First(
                multiplicative,
                ThrowError("Expected multiplicative")
            );

            var additive = LeftApply(
                multiplicative,
                left => First(
                    Rule(
                        left,
                        Token(TokenType.Addition),
                        requiredMultiplicative,
                        (l, op, r) => l + r
                    ),
                    Rule(
                        left,
                        Token(TokenType.Subtraction),
                        requiredMultiplicative,
                        (l, op, r) => l - r
                    )
                )
            );

            var requiredEnd = If(
                End(),
                Produce(() => Defaults.ObjectInstance),
                Produce<object>(state => throw new Exception($"Expected end of input but found {state.Input.Peek()} at {state.Input.CurrentLocation}"))
            );

            var expression = Rule(
                additive,
                requiredEnd,
                (add, end) => add
            );

            return expression;
        }
    }
}
