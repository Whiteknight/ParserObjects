﻿namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public class Token
    {
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public TokenType Type { get; }
        public string Value { get; }

        public override string ToString() => $"{Type}: {Value}";
    }
}