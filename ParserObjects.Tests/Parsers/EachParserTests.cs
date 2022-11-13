using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EachParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Each(
                Produce(() => 'A'),
                Produce(() => 'B'),
                Produce(() => 'C')
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be('A');
            result.Results[1].Value.Should().Be('B');
            result.Results[2].Value.Should().Be('C');
        }

        [Test]
        public void ToBnf_Test_1()
        {
            var target = Each(
                Produce(() => 'A')
            );
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := PRODUCE");
        }

        [Test]
        public void ToBnf_Test_3()
        {
            var target = Each(
                Produce(() => 'A'),
                Produce(() => 'B'),
                Produce(() => 'C')
            );
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := EACH(PRODUCE | PRODUCE | PRODUCE)");
        }
    }
}
