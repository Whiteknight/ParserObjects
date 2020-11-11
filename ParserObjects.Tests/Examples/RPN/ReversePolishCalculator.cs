﻿using System;
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
            Operator
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
                operators
            );
            var tokenSequence = tokens.ToSequence(new StringCharacterSequence(s)).Select(r => r.Value);
            var result = tokenSequence.Parse<RpnToken, int>((t, success, fail) =>
            {
                var startingLocation = t.CurrentLocation;
                var stack = new Stack<int>();
                while (!t.IsAtEnd)
                {
                    var token = t.GetNext();
                    if (token == null)
                        return fail();
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
                        continue;
                    }
                }
                return success(stack.Pop());
            });

            if (!result.Success)
                throw new Exception("Parse failed");
            return result.Value;
        }
    }
}
