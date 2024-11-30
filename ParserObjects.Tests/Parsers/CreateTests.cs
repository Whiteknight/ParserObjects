using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class CreateTests
{
    public class Single
    {
        [Test]
        public void Parse_Test()
        {
            var target = Create(_ => Produce(() => 'A'));
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            result.Value.Should().Be('A');
        }

        [Test]
        public void Parse_Untyped()
        {
            IParser<char> target = Create(_ => Produce(() => 'A'));
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            result.Value.Should().Be('A');
        }

        [Test]
        public void Parse_InnerFails()
        {
            var target = Create(_ => Fail<char>());
            var result = target.Parse(FromString(""));
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_InnerReturnsNull()
        {
            var target = Create(_ => (IParser<char, char>)null);
            Action act = () => target.Parse(FromString(""));
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Parse_CallbackConsumes()
        {
            // Read a char from input and use that to create the parser.
            // The Consumed value of the result should be 2: 1 to create the parser
            // and 1 consumed by the parser itself
            var target = Create(state => MatchChar(state.Input.GetNext()));
            var result = target.Parse(FromString("AA"));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(2);
            result.Value.Should().Be('A');
        }

        [Test]
        public void Match_Test()
        {
            var target = Create(_ => Produce(() => 'A'));
            var result = target.Match(FromString(""));
            result.Should().BeTrue();
        }

        [Test]
        public void Match_CallbackConsumes_False()
        {
            // We consume 1 item of input from the sequence to create the parser
            // When the Inner parser fails, we need to make sure the 1 input item is
            // returned.
            var target = Create(state =>
            {
                state.Input.GetNext();
                return Fail<char>();
            });
            var sequence = FromString("AA");
            var result = target.Match(sequence);
            result.Should().BeFalse();
            sequence.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Create(_ => Any()).Named("parser");
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Create(_ => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := CREATE");
        }

        [Test]
        public void Parse_ConsumeInput()
        {
            var target = Create(state => Produce(() => state.Input.GetNext()));
            var result = target.Parse(FromString("A"));
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
            result.Value.Should().Be('A');
        }
    }

    public class Multi
    {
        [Test]
        public void Parse_Test()
        {
            var target = CreateMulti(_ => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == "a");
            result.Results.Should().Contain(r => r.Value == "b");
            result.Results.Should().Contain(r => r.Value == "c");
        }

        [Test]
        public void Parse_ConsumeInputs()
        {
            var target = CreateMulti(s => ProduceMulti(() => new[] { s.Input.GetNext(), 'b', 'c' }));
            var result = target.Parse(FromString("a"));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results.Should().Contain(r => r.Value == 'a');
            result.Results.Should().Contain(r => r.Value == 'b');
            result.Results.Should().Contain(r => r.Value == 'c');
        }

        [Test]
        public void Parse_Untyped()
        {
            IMultiParser<char> target = CreateMulti(_ => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.Parse(FromString(""));
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
        }

        [Test]
        public void Parse_InnerFails()
        {
            var target = CreateMulti(_ => FailMulti<char>());
            var result = target.Parse(FromString(""));
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_InnerFails_CallbackConsumes()
        {
            var target = CreateMulti(state =>
            {
                state.Input.GetNext();
                return FailMulti<char>();
            });
            var sequence = FromString("abc");
            var result = target.Parse(sequence);
            result.Success.Should().BeFalse();
            sequence.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = CreateMulti(_ => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = CreateMulti(_ => ProduceMulti(() => new[] { "a", "b", "c" }));
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := CREATE");
        }

        [Test]
        public void Parse_ConsumeInput()
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
