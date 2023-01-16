using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class SetResultDataTests
{
    public class DirectValue
    {
        [Test]
        public void Parse_Test()
        {
            var target = Rule(
                Produce(() => "value").SetResultData("test"),
                GetData<string>("test"),
                (a, b) => $"{a}{b}"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("valuevalue");
        }

        [Test]
        public void ToBnf_Test()
        {
            // TODO: We want this to be better
            var target = Any().SetResultData("test").Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := SetResultData");
        }
    }

    public class DerivedValue
    {
        [Test]
        public void Parse_Test()
        {
            var target = Rule(
                Produce(() => "value").SetResultData("test", s => s.Length),
                GetData<int>("test"),
                (a, b) => $"{a}{b}"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("value5");
        }
    }
}
