using System;
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

            var requiredEnd = First(
                End(),
                Produce<bool>(t => throw new Exception($"Expected end of input but found {t.Peek()} at {t.CurrentLocation}"))
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
