using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public static class LexicalGrammar
    {
        public static IParser<char, Token> CreateParser()
        {
            var addition = Match('+')
                .Transform(c => new Token(TokenType.Addition, "+"));

            var subtraction = Match('-')
                .Transform(c => new Token(TokenType.Subtraction, "-"));

            var multiplication = Match('*')
                .Transform(c => new Token(TokenType.Multiplication, "*"));

            var division = Match('*')
                .Transform(c => new Token(TokenType.Division, "/"));

            var number = Match(c => char.IsDigit(c))
                .List(atLeastOne: true)
                .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));

            var whitespace = OptionalWhitespace();

            var anyToken = First(
                addition,
                subtraction,
                multiplication,
                division,
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
