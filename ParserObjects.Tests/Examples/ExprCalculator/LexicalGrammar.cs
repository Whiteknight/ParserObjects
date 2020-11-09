using System.Linq;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public static class LexicalGrammar
    {
        public static IParser<char, Token> CreateParser()
        {
            var addition = Match('+')
                .Transform(c => new Token(TokenType.Addition, "+"));
            var multiplication = Match('*')
                .Transform(c => new Token(TokenType.Multiplication, "*"));
            var number = Match(c => char.IsDigit(c))
                .List(atLeastOne: true)
                .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));
            var whitespace = Match(c => char.IsWhiteSpace(c)).List();
            var anyToken = First(
                addition,
                multiplication,
                number
            );

            var whitespaceAndToken = Rule(
                whitespace,
                anyToken,
                (ws, t) => t
            );
            return whitespaceAndToken;
        }
    }
}
