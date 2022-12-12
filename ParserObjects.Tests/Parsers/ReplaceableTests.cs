using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public static class ReplaceableTests
    {
        public class SingleOutput
        {
            [Test]
            public void Parse_Test()
            {
                var anyParser = Any();
                var target = Replaceable(anyParser);
                var input = FromString("abc");
                var result = target.Parse(input);
                result.Value.Should().Be('a');
                result.Consumed.Should().Be(1);
            }

            [Test]
            public void Parse_Fail()
            {
                var anyParser = Fail<char>();
                var target = Replaceable(anyParser);
                var input = FromString("abc");
                var result = target.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void Parse_Null()
            {
                var target = Replaceable((IParser<char, char>)null);
                var input = FromString("abc");
                var result = target.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void SetParser_Test()
            {
                var anyParser = Any();
                var failParser = Fail<char>();
                var target = Replaceable(failParser);
                (target as IReplaceableParserUntyped)?.SetParser(anyParser);
                var input = FromString("abc");
                var result = target.Parse(input);
                result.Value.Should().Be('a');
            }

            [Test]
            public void GetChildren_Test()
            {
                var anyParser = Any();
                var target = Replaceable(anyParser);
                var result = target.GetChildren().ToList();
                result.Count.Should().Be(1);
                result[0].Should().BeSameAs(anyParser);
            }

            [Test]
            public void ToBnf_Test()
            {
                var parser = Replaceable(Any()).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := .");
            }
        }

        public class SingleNoOutput
        {
            [Test]
            public void Parse_Test()
            {
                var target = Replaceable(End());
                var input = FromString("");
                var result = target.Parse(input);
                result.Success.Should().BeTrue();
            }

            [Test]
            public void GetChildren_Test()
            {
                var end = End();
                var target = Replaceable(end);
                var result = target.GetChildren().ToList();
                result.Count.Should().Be(1);
                result[0].Should().BeSameAs(end);
            }

            [Test]
            public void Parse_Null()
            {
                var target = Replaceable((IParser<char>)null);
                var input = FromString("abc");
                var result = target.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void ToBnf_Test()
            {
                var parser = Replaceable(End()).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := END");
            }
        }

        public class Multi
        {
            [Test]
            public void ToBnf_Test()
            {
                var parser = Replaceable(ProduceMulti(() => new char[0])).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := PRODUCE");
            }
        }
    }
}
