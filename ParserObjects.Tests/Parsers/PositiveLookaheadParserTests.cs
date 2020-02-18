using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class PositiveLookaheadParserTests
    {
        [Test]
        public void FollowedBy_Fail()
        {
            var parser = Match('[').FollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[test]");
            parser.Parse(input).Success.Should().BeFalse();
            
        }

        [Test]
        public void FollowedBy_Success()
        {
            var parser = Match('[').FollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[~test]");
            parser.Parse(input).Value.Should().Be('[');
        }
    }
}