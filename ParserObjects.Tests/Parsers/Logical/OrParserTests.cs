using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class OrParserTests
    {
        private readonly IParser<char, bool> _trueParser = Produce<char, bool>(() => true);
        private readonly IParser<char, bool> _falseParser = Produce<char, bool>(() => false);
        private readonly IParser<char, bool> _failParser = Fail<char, bool>();

        [Test]
        public void Parse_True_True()
        {
            var parser = Or(_trueParser, _trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_True_False()
        {
            var parser = Or(_trueParser, _falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_False_True()
        {
            var parser = Or(_falseParser, _trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_False_False()
        {
            var parser = Or(_falseParser, _falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_True_Fail()
        {
            var parser = Or(_trueParser, _failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_Fail_True()
        {
            var parser = Or(_failParser, _trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_False_Fail()
        {
            var parser = Or(_falseParser, _failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_False()
        {
            var parser = Or(_failParser, _falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild_1()
        {
            var parser = Or(_falseParser, _failParser);
            parser = parser.ReplaceChild(_failParser, _trueParser) as IParser<char, bool>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_ReplaceChild_2()
        {
            var parser = Or(_failParser, _falseParser);
            parser = parser.ReplaceChild(_failParser, _trueParser) as IParser<char, bool>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }
}