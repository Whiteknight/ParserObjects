using static ParserObjects.Parsers;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

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

            var operation = operators.Chain(r =>
            {
                if (!r.Success)
                    return Fail<int>("Unrecognized operator");
                var op = r.Value;
                if (op == '+')
                    return operands.Transform(v => v[0] + v[1]);
                if (op == '-')
                    return operands.Transform(v => v[0] - v[1]);
                if (op == '*')
                    return operands.Transform(v => v[0] * v[1]);
                if (op == '/')
                    return operands.Transform(v => v[0] / v[1]);
                return Fail<int>("Unrecognized operator");
            });

            valueInternal = First(
                operation,
                integer
            );

            return Rule(
                OptionalWhitespace(),
                valueInternal,
                (ws, v) => v
            ).FollowedBy(End());
        }
    }
}
