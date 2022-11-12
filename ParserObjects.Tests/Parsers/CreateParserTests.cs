using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

namespace ParserObjects.Tests.Parsers
{
    public class CreateParserTests
    {
        [Test]
        public void Parse_Single()
        {
            var target = Create(state => Produce(() => 'A'));
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            result.Value.Should().Be('A');
        }

        [Test]
        public void Parse_Multi()
        {
            var target = CreateMulti(state => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.Parse(FromString(""));
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

        [Test]
        public void Parse_ConsumeInput_Single()
        {
            var target = Create(state => Produce(() => state.Input.GetNext()));
            var result = target.Parse(FromString("A"));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
            result.Value.Should().Be('A');
        }

        [Test]
        public void Parse_ConsumeInput_Multi()
        {
            var target = CreateMulti(state => ProduceMulti(() => new[] { state.Input.GetNext(), state.Input.GetNext(), 'c' }));
            var result = target.Parse(FromString("AB"));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be('A');
            result.Results[0].Consumed.Should().Be(2);
            result.Results[1].Value.Should().Be('B');
            result.Results[1].Consumed.Should().Be(2);
            result.Results[2].Value.Should().Be('c');
            result.Results[2].Consumed.Should().Be(2);
        }
    }
}
