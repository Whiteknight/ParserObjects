using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public static class LexicalGrammar
    {
        public static IParser<char, Token> CreateParser()
        {
            var addition = Match('+')
                .Transform(_ => new Token(TokenType.Addition, "+"));

            var subtraction = Match('-')
                .Transform(_ => new Token(TokenType.Subtraction, "-"));

            var multiplication = Match('*')
                .Transform(_ => new Token(TokenType.Multiplication, "*"));

            var division = Match('*')
                .Transform(_ => new Token(TokenType.Division, "/"));

            var exponent = Match('^')
                .Transform(_ => new Token(TokenType.Exponentiation, "^"));

            var factorial = Match('!')
                .Transform(_ => new Token(TokenType.Factorial, "!"));

            var openParen = Match('(')
                .Transform(_ => new Token(TokenType.OpenParen, "("));

            var closeParen = Match(')')
                .Transform(_ => new Token(TokenType.CloseParen, ")"));

            var number = Match(c => char.IsDigit(c))
                .List(atLeastOne: true)
                .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));

            var end = If(End(), Produce(() =>
                new Token(TokenType.End, "")));

            var whitespace = OptionalWhitespace();

            var anyToken = First(
                addition,
                subtraction,
                multiplication,
                division,
                exponent,
                factorial,
                number,
                openParen,
                closeParen,
                end
            );

            var whitespaceAndToken = Rule(
                whitespace,
                anyToken,
                (_, t) => t
            );

            return whitespaceAndToken;
        }
    }
}
