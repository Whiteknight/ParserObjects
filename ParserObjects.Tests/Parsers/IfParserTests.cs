using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class IfParserTests
    {
        private readonly IParser<char, char> _successParser = Any();
        private readonly IParser<char, bool> _failParser = Fail<bool>();

        [Test]
        public void Ext_Then_Success()
        {
            var parser = _successParser.Then(Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_Then_Fail()
        {
            var parser = _failParser.Then(Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Ext_If_Success()
        {
            var parser = Any().If(_successParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_If_Fail()
        {
            var parser = Any().If(_failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Success()
        {
            var parser = If(_successParser, Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = If(_failParser, Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void If_ReplaceChild_1()
        {
            var parser = If(_failParser, Any());
            parser = parser.ReplaceChild(_failParser, _successParser) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void If_ReplaceChild_2()
        {
            var empty = Empty().Transform(c => '\0');
            var any = Any();
            var parser = If(_successParser, empty);
            parser = parser.ReplaceChild(empty, any) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }
}
