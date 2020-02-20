using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Parsers.Logical;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class IfParserTests
    {
        private readonly IParser<char, bool> _trueParser = Produce<char, bool>(() => true);
        private readonly IParser<char, bool> _falseParser = Produce<char, bool>(() => false);

        [Test]
        public void Ext_Then_Match()
        {
            var parser = _trueParser.Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_Then_NotMatch()
        {
            var parser = _falseParser.Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Ext_If_Match()
        {
            var parser = Any<char>().If(_trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_If_NotMatch()
        {
            var parser = Any<char>().If(_falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void If_Match()
        {
            var parser = If(_trueParser, Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void If_NotMatch()
        {
            var parser = If(_falseParser, Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void If_ReplaceChild_1()
        {
            var parser = If(_falseParser, Any<char>());
            parser = parser.ReplaceChild(_falseParser, _trueParser) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void If_ReplaceChild_2()
        {
            var empty = Empty<char>().Transform(c => '\0');
            var any = Any<char>();
            var parser = If(_trueParser, empty);
            parser = parser.ReplaceChild(empty, any) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }
}
