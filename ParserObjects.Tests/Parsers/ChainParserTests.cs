using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChainParserTests
    {
        [Test]
        public void Parse_Basic()
        {
            var parser = Any().Chain(c =>
            {
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Y');

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();

            result = parser.Parse("cZ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Z');
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Fail<object>().Chain(c => Any());
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild()
        {
            var initial = Fail<char>();
            var parser = initial.Chain(c => Any());
            parser = parser.ReplaceChild(initial, Any()) as IParser<char, char>;
            var result = parser.Parse("ab");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('b');
        }
    }
}
