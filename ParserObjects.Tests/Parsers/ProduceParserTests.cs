using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class ProduceParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Produce(() => 5);
            var input = FromString("abc");
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
        public void ProduceMulti_NoArgs()
        {
            var target = ProduceMulti(() => new[] { "a", "b", "c" });
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == "a");
            result.Results.Should().Contain(r => r.Value == "b");
            result.Results.Should().Contain(r => r.Value == "c");
        }

        [Test]
        public void ProduceMulti_Args_GetNextInput()
        {
            var target = ProduceMulti(state =>
            {
                var x = state.Input.GetNext();
                return new[] { x, 'b', 'c' };
            });
            var result = target.Parse(FromString("X"));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be('X');
            result.Results[0].Consumed.Should().Be(1);
            result.Results[1].Value.Should().Be('b');
            result.Results[1].Consumed.Should().Be(1);
            result.Results[2].Value.Should().Be('c');
            result.Results[2].Consumed.Should().Be(1);
        }

        [Test]
        public void ProduceMulti_Args_GetData()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    var x = state.Data.Get<char>("Value").Value;
                    return new[] { x, 'b', 'c' };
                }),
                "Value",
                'X'
            );
            var result = target.Parse(FromString("X"));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be('X');
            result.Results[0].Consumed.Should().Be(0);
            result.Results[1].Value.Should().Be('b');
            result.Results[1].Consumed.Should().Be(0);
            result.Results[2].Value.Should().Be('c');
            result.Results[2].Consumed.Should().Be(0);
        }
    }
}
