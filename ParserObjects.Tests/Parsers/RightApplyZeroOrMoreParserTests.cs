using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class RightApplyZeroOrMoreParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("1a2b3c4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1a(2b(3c4)))");
        }

        [Test]
        public void ParseUntyped_Test()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("1a2b3c4");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1a(2b(3c4)))");
        }

        [Test]
        public void Parse_Fail_MissingFirst()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("X");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_MissingMiddle()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("1");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void Parse_MissingRight_Synthetic()
        {
            // If we match <first> and <middle> but fail to parse <right>, we have an option
            // to generate a synthetic right production and continue the parse, so we don't
            // have a dangling <middle> to be accounted for later.
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})",
                t => "X"
            );

            var input = new StringCharacterSequence("1a");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1aX)");

        }

        [Test]
        public void Parse_MissingRight_Rewind()
        {
            // If we match <first> and <middle> but fail to parse <right> and there is
            // no synthetic option specified, we should rewind <middle> and only return
            // <first>
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("1a");
            var result = parser.Parse(input);
            result.Value.Should().Be("1");
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_MissingRight_Recursed_Rewind()
        {
            // We match <first> and <middle>, recurse on <right>, but the recursed rule
            // fails on <recursed.Right>. Rewind back to a success and leave the 
            // unmatched second <middle> on the input sequence
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );

            var input = new StringCharacterSequence("1a2b");
            var result = parser.Parse(input);
            result.Value.Should().Be("(1a2)");
            input.Peek().Should().Be('b');
        }

        [Test]
        public void Parse_MissingRight_Recursed_Synthetic()
        {
            // We match <first> and <middle>, recurse on <right>, but the recursed rule
            // fails on <recursed.Right> so we use the fallback to produce a synthetic
            // <right>. No rewind.
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})",
                t => "X"
            );

            var input = new StringCharacterSequence("1a2b");
            var result = parser.Parse(input);
            result.Value.Should().Be("(1a(2bX))");
        }

        [Test]
        public void ReplaceChild_Item()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var dotParser = Match<char>(".").Transform(c => c[0].ToString());

            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );
            parser = parser.ReplaceChild(numberParser, dotParser) as IParser<char, string>;

            var input = new StringCharacterSequence(".a.b.c.");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(.a(.b(.c.)))");
        }

        [Test]
        public void ReplaceChild_Middle()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());
            var dotParser = Match<char>(".").Transform(c => c[0].ToString());

            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );
            parser = parser.ReplaceChild(letterParser, dotParser) as IParser<char, string>;

            var input = new StringCharacterSequence("1.2.3.4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1.(2.(3.4)))");
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());

            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );
            var result = parser.ReplaceChild(null, null);
            result.Should().BeSameAs(parser);
        }

        [Test]
        public void GetChildren_Test()
        {
            var numberParser = Match<char>(char.IsNumber).Transform(c => c.ToString());
            var letterParser = Match<char>(char.IsLetter).Transform(c => c.ToString());

            var parser = RightApply(
                numberParser,
                letterParser,
                (l, m, r) => $"({l}{m}{r})"
            );
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(2);
            result[0].Should().BeSameAs(numberParser);
            result[1].Should().BeSameAs(letterParser);
        }
    }
}