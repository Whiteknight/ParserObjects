using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods<char>;
using static ParserObjects.Parsers.Specialty.DigitParserMethods;
using static ParserObjects.Parsers.Specialty.WhitespaceParserMethods;

namespace ParserObjects.Tests.Examples.PN
{
    public static class PolishGrammar
    {
        public static IParser<char, int> GetParser()
        {
            var ws = Whitespace();
            var ows = OptionalWhitespace();
            var integer = DigitString().Transform(int.Parse);

            IParser<char, int> valueInternal = null;

            var value = Rule(
                Deferred(() => valueInternal),
                OptionalWhitespace(),

                (v, ws) => v
            );

            var operators = Rule(
                First(
                    Match('+'),
                    Match('-'),
                    Match('*'),
                    Match('/')
                ),
                OptionalWhitespace(),

                (op, ws) => op
            );

            var operands = Rule(
                value,
                value,
                (a, b) => new[] { a, b }
            );

            var operation = operators.Chain(op =>
            {
                if (op == '+')
                    return operands.Transform(v => v[0] + v[1]);
                if (op == '-')
                    return operands.Transform(v => v[0] - v[1]);
                if (op == '*')
                    return operands.Transform(v => v[0] * v[1]);
                if (op == '/')
                    return operands.Transform(v => v[0] / v[1]);
                throw new System.Exception("Unrecognized operator");
            });

            valueInternal = First(
                operation,
                integer
            );

            return Rule(
                OptionalWhitespace(),
                valueInternal,
                End(),

                (ws, v, end) => v
            );
        }
    }
}

