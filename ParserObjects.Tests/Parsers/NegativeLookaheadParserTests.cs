using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class NegativeLookaheadParserTests
    {
        [Test]
        public void NotFollowedBy_Success()
        {
            var parser = Match('[').NotFollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[test]");
            parser.Parse(input).Value.Should().Be('[');
        }

        [Test]
        public void NotFollowedBy_Fail()
        {
            var parser = Match('[').NotFollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[~test]");
            parser.Parse(input).Success.Should().BeFalse();
        }
    }
}
