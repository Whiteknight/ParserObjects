using System;
using System.Collections.Generic;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Bnf;

public static class BnfDescription
{
    public static void Append(BnfStringifyState state, string description, IReadOnlyList<IParser> children)
    {
        var parser = Grammar.GetParser();
        var input = ParserObjects.Sequences.FromString(description);
        while (!input.IsAtEnd)
        {
            var result = parser.Parse(input);
            if (!result.Success)
            {
                state.Append("ERROR");
                break;
            }

            var token = result.Value;
            switch (token.Type)
            {
                case TokenType.Literal:
                    state.Append(token.Literal);
                    break;

                case TokenType.Child:
                    if (token.ChildIndex < children.Count)
                        state.Append(children[token.ChildIndex]);
                    break;
            }
        }
    }

    public enum TokenType
    {
        Literal,
        Child,
        End
    }

    public readonly record struct Token(TokenType Type, string Literal, int ChildIndex);

    public static class Grammar
    {
        public static IParser<char, Token> GetParser() => _parser.Value;

        private static readonly Lazy<IParser<char, Token>> _parser = new Lazy<IParser<char, Token>>(() =>
        {
            var child = Rule(
                MatchChar('%'),
                DigitsAsInteger(),
                (_, i) => new Token(TokenType.Child, string.Empty, i)
            );
            var regularChars = First(
                MatchChar(c => c != '%'),
                MatchChar('%').NotFollowedBy(Digit())
            );
            var literal = regularChars
                .ListCharToString(1)
                .Transform(s => new Token(TokenType.Literal, s, 0));
            return First(
                If(End(), Produce(() => new Token(TokenType.End, string.Empty, 0))),
                child,
                literal
            );
        });
    }
}
