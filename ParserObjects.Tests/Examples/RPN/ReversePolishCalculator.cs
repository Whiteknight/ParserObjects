using System;
using System.Collections.Generic;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.RPN
{
    public class ReversePolishCalculator
    {
        private enum RpnTokenType
        {
            Number,
            Operator,
            End,
            Failure
        }

        private class RpnToken
        {
            public RpnToken(string value, RpnTokenType type)
            {
                Value = value;
                Type = type;
            }

            public string Value { get; }
            public RpnTokenType Type { get; }
        }

        public int Calculate(string s)
        {
            var ws = Whitespace();
            var ows = OptionalWhitespace();

            var integer = Rule(
                DigitString(),
                ows,
                (d, ws) => new RpnToken(d, RpnTokenType.Number)
            );
            var operators = Rule(
                First(
                    Match('+'),
                    Match('-'),
                    Match('*'),
                    Match('/')
                ),
                OptionalWhitespace(),
                (op, ws) => new RpnToken(op.ToString(), RpnTokenType.Operator)
            );
            var tokens = First(
                integer,
                operators,
                If(End(), Produce(() => new RpnToken("", RpnTokenType.End)))
            );
            var tokenSequence = tokens
                .ToSequence(new StringCharacterSequence(s))
                .Select(r => r.GetValueOrDefault(() => new RpnToken(r.ErrorMessage, RpnTokenType.Failure)));

            var parser = ParserMethods<RpnToken>.Function<int>((t, success, fail) =>
            {
                var startingLocation = t.Input.CurrentLocation;
                var stack = new Stack<int>();
                while (!t.Input.IsAtEnd)
                {
                    var token = t.Input.GetNext();
                    if (token == null)
                        return fail("Received null token");
                    if (token.Type == RpnTokenType.Failure)
                        return fail("Tokenization error: " + token.Value);
                    if (token.Type == RpnTokenType.End)
                        break;
                    if (token.Type == RpnTokenType.Number)
                    {
                        stack.Push(int.Parse(token.Value));
                        continue;
                    }

                    if (token.Type == RpnTokenType.Operator)
                    {
                        var b = stack.Pop();
                        var a = stack.Pop();
                        switch (token.Value)
                        {
                            case "+":
                                stack.Push(a + b);
                                break;

                            case "-":
                                stack.Push(a - b);
                                break;

                            case "*":
                                stack.Push(a * b);
                                break;

                            case "/":
                                stack.Push(a / b);
                                break;
                        }
                    }
                }

                if (stack.Count != 1)
                    return fail("Invalid sequence, expected to have 1 token remaining");
                return success(stack.Pop());
            });
            var result = parser.Parse(tokenSequence);
            if (!result.Success)
                throw new Exception("Parse failed");
            return result.Value;
        }
    }
}
