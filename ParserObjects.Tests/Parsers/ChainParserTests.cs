using System;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChainParserTests
    {
        [Test]
        public void Parse_Basic()
        {
            var parser = Any().Chain(r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
            result.Consumed.Should().Be(2);

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Y');
            result.Consumed.Should().Be(2);

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("cZ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Z');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Fail<object>().Chain(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(false);
        }

        [Test]
        public void Parse_Throw()
        {
            var parser = Any().Chain<char, char, object>(c => throw new System.Exception());
            var input = new StringCharacterSequence("abc");
            Action act = () => parser.Parse(input);
            act.Should().Throw<Exception>();
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }
    }
}
