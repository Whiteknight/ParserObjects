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
            var target = Produce(() => 5);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be(5);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Produce(() => 5);
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void ProduceMulti_Test()
        {
            var target = ProduceMulti(() => new[] { "a", "b", "c" });
            var result = target.Parse(new StringCharacterSequence(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == "a");
            result.Results.Should().Contain(r => r.Value == "b");
            result.Results.Should().Contain(r => r.Value == "c");
        }
    }
}
