using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ProduceParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Produce(x => 5);
            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be(5);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Produce(x => 5);
            target.GetChildren().Count().Should().Be(0);
        }
    }
}
