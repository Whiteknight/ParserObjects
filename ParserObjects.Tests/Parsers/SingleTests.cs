using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Multi
{
    public class SingleTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = ProduceMulti(() => "a").Single();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_FailNoResults()
        {
            var target = ProduceMulti(() => "").Single();
            var result = target.Parse("");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_FailTooMany()
        {
            var target = ProduceMulti(() => "abc").Single();
            var result = target.Parse("");
            result.Success.Should().BeFalse();
        }
    }
}
