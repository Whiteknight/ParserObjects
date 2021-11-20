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
            var target = CreateMulti(state => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.Parse(new StringCharacterSequence(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == "a");
            result.Results.Should().Contain(r => r.Value == "b");
            result.Results.Should().Contain(r => r.Value == "c");
        }

        [Test]
        public void ToBnf_Single()
        {
            var parser = Create(state => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := CREATE");
        }

        [Test]
        public void ToBnf_Multi()
        {
            var target = CreateMulti(state => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := CREATE");
        }

        // TODO: Tests where we consume input during the create delegate
    }
}
