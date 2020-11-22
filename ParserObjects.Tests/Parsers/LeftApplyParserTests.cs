using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
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
        }

        [Test]
        public void ZeroOrMore_Parse_NonReference()
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
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var numberParser = Match(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match(char.IsLetter).Transform(c => c.ToString());
            var dotParser = Match(".").Transform(c => c[0].ToString());
            var parser = LeftApply(
                numberParser,
                left => Rule(
                    left,
                    letterParser,
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                )
            );
            // Replaces only the first numberParser, not the one inside the child parser
            var result = parser.ReplaceChild(null, null) as IParser<char, string>;
            result.Should().BeSameAs(parser);
        }

        [Test]
        public void ReplaceChild_Item()
        {
            var numberParser = Match(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match(char.IsLetter).Transform(c => c.ToString());
            var dotParser = Match(".").Transform(c => c[0].ToString());
            var parser = LeftApply(
                numberParser,
                left => Rule(
                    left,
                    letterParser,

                    // In order to replace this numberParser instance, we would need to use a replace
                    // visitor to recurse the structure
                    numberParser,
                    (l, op, r) => $"({l}{op}{r})"
                )
            );
            // Replaces only the first numberParser, not the one inside the child parser
            parser = parser.ReplaceChild(numberParser, dotParser) as IParser<char, string>;

            var input = new StringCharacterSequence(".a2b3c4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(((.a2)b3)c4)");
        }

        [Test]
        public void ReplaceChild_Right()
        {
            var numberParser = Match(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match(char.IsLetter).Transform(c => c.ToString());
            var dotParser = Match(".").Transform(c => c[0].ToString());

            var parser = LeftApply(
                numberParser,
                left => letterParser
            );
            parser = parser.ReplaceChild(letterParser, dotParser) as IParser<char, string>;

            var input = new StringCharacterSequence("1.");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(".");
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
