using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class AndParserTests
    {
        [Test]
        public void Parse_True_True()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(trueParser, trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_True_False()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(trueParser, falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_False_True()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(falseParser, trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_False_False()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(falseParser, falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_True_Fail()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var failParser = ParserMethods.Fail<char, bool>();
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(trueParser, failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_True()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var failParser = ParserMethods.Fail<char, bool>();
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(failParser, trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_False_Fail()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var failParser = ParserMethods.Fail<char, bool>();
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(falseParser, failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_False()
        {
            var trueParser = ParserMethods.Produce<char, bool>(() => true);
            var falseParser = ParserMethods.Produce<char, bool>(() => false);
            var failParser = ParserMethods.Fail<char, bool>();
            var parser = ParserObjects.Parsers.Logical.ParserMethods.And(failParser, falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }
    }
}