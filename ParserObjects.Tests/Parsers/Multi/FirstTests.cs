using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Multi
{
    public class FirstTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = ProduceMulti(() => "a").First();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Test_Predicate()
        {
            var target = ProduceMulti(() => "a").First(r => r.Success);
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_FailNoResults()
        {
            var target = ProduceMulti(() => "").First();
            var result = target.Parse("");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_FailNoResults_Predicate()
        {
            var target = ProduceMulti(() => "").First(r => r.Success);
            var result = target.Parse("");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_FailTooMany()
        {
            var target = ProduceMulti(() => "abc").First();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_FailTooMany_Predicate()
        {
            var target = ProduceMulti(() => "abc").First(r => r.Success);
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }
}
