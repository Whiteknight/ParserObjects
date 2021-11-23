using System.Linq;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ReplaceableParserTests
    {
        [Test]
        public void Parse_SingleOutput()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_SingleNoOutput()
        {
            var target = Replaceable(End());
            var input = new StringCharacterSequence("");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Parse_SingleOutput_Fail()
        {
            var anyParser = Fail<char>();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void SetParser_SingleOutput_Test()
        {
            var anyParser = Any();
            var failParser = Fail<char>();
            var target = new Replaceable<char, char>.SingleParser(failParser);
            target.SetParser(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void GetChildren_SingleOutput_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void GetChildren_SingleNoOutput_Test()
        {
            var end = End();
            var target = Replaceable(end);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(end);
        }

        [Test]
        public void ToBnf_Single()
        {
            var parser = Replaceable(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_Multi()
        {
            var parser = Replaceable(ProduceMulti(() => new char[0])).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }
    }
}
