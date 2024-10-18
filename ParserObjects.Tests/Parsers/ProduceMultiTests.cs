using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class ProduceMultiTests
{
    public class NoArgs
    {
        [Test]
        public void Parse_NoArgs()
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
        public void Match_Test()
        {
            var target = ProduceMulti(() => new[] { "a", "b", "c" });
            var input = FromString("");
            var result = target.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = ProduceMulti(() => "abc").Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := PRODUCE");
        }
    }

    public class Args
    {
        [Test]
        public void Parse_Args_GetNext()
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
        public void Parse_Args_GetData()
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
