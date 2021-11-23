using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class LeftApplyParserTests
    {
        [Test]
        public void ZeroOrMore_Parse_Test()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                )
            );

            var input = new StringCharacterSequence("1a2b3c4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(((1a2)b3)c4)");
            result.Consumed.Should().Be(7);
        }

        [Test]
        public void ZeroOrOne_Parse_Test()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ZeroOrOne
            );

            var input = new StringCharacterSequence("1a2b3c4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1a2)");
            result.Consumed.Should().Be(3);
        }

        [Test]
        public void ExactlyOne_Parse_Test()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ExactlyOne
            );

            var input = new StringCharacterSequence("1a2b3c4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1a2)");
            result.Consumed.Should().Be(3);
        }

        [Test]
        public void ZeroOrMore_Parse_NoLeftParser()
        {
            var numberParser = Match(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match(char.IsLetter).Transform(c => c.ToString());

            // Just use a parser not a reference to build a parser. Since we aren't composing
            // partial results, the output will just be the last letter
            var parser = LeftApply(
                numberParser,
                left => letterParser
            );

            var input = new StringCharacterSequence("1abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("c");
            result.Consumed.Should().Be(4);
        }

        [Test]
        public void ZeroOrMore_Parse_FailLeft()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                )
            );

            var input = new StringCharacterSequence("X");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void ExactlyOne_Parse_FailLeft()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ExactlyOne
            );

            var input = new StringCharacterSequence("X");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void ZeroOrOne_Parse_FailLeft()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ZeroOrOne
            );

            var input = new StringCharacterSequence("X");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void ExactlyOne_Parse_FailNoRight()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ExactlyOne
            );

            var input = new StringCharacterSequence("1");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void ZeroOrOne_Parse_NoRight()
        {
            var numberParser = Match(char.IsNumber);
            var letterParser = Match(char.IsLetter);
            var parser = LeftApply(
                numberParser.Transform(c => c.ToString()),
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                ),
                Quantifier.ZeroOrOne
            );

            var input = new StringCharacterSequence("1");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void GetChildren_Test()
        {
            var numberParser = Match(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match(char.IsLetter).Transform(c => c.ToString());

            // Just use a parser not a reference to build a parser. Since we aren't composing
            // partial results, the output will just be the last letter
            var parser = LeftApply(
                numberParser,
                left => letterParser
            );
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(2);
            result[0].Should().BeSameAs(numberParser);
            result[1].Should().BeSameAs(letterParser);
        }
    }
}
