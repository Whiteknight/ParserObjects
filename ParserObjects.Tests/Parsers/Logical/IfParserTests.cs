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
        private readonly IParser<char, char> _successParser = Any<char>();
        private readonly IParser<char, bool> _failParser = Fail<char, bool>();

        [Test]
        public void Ext_Then_Success()
        {
            var parser = _successParser.Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_Then_Fail()
        {
            var parser = _failParser.Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Ext_If_Success()
        {
            var parser = Any<char>().If(_successParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_If_Fail()
        {
            var parser = Any<char>().If(_failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Success()
        {
            var parser = If(_successParser, Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = If(_failParser, Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void If_ReplaceChild_1()
        {
            var parser = If(_failParser, Any<char>());
            parser = parser.ReplaceChild(_failParser, _successParser) as IParser<char, char>;

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
            var parser = If(_successParser, empty);
            parser = parser.ReplaceChild(empty, any) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }
}
