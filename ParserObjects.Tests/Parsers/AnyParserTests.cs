using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class AnyParserTests
    {
        [Test]
        public void Any_Char()
        {
            var target = new AnyParser<char>();
            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('b');
            target.Parse(input).Value.Should().Be('c');
            target.Parse(input).Success.Should().BeFalse();
        }
    }
}
