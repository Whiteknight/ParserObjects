using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class CreateParserTests
    {
        [Test]
        public void Parse_Multi()
        {
            var target = CreateMulti((i, d) => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.Parse(new StringCharacterSequence(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == "a");
            result.Results.Should().Contain(r => r.Value == "b");
            result.Results.Should().Contain(r => r.Value == "c");
        }
    }
}
