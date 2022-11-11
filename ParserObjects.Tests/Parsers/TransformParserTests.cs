using System.Linq;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class TransformParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var any = Any();
            var parser = Transform(
                any,
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Value.Should().Be(1);
            result.Parser.Should().BeSameAs(parser);
        }

        [Test]
        public void Parse_Multi()
        {
            var parser = Transform(
                ProduceMulti(() => new[] { "1" }),
                c => int.Parse(c.ToString())
            );
            parser.Parse("").Results[0].Value.Should().Be(1);
        }

        [Test]
        public void Parse_Extension()
        {
            var parser = Any().Transform(c => int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void Parse_Extension_Multi()
        {
            var parser = ProduceMulti(() => new[] { "1" }).Transform(c => int.Parse(c.ToString()));
            parser.Parse("").Results[0].Value.Should().Be(1);
        }

        [Test]
        public void Parse_Extension_Map()
        {
            var parser = Any().Map(c => int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void Parse_Failure()
        {
            var fail = Fail<char>();
            var parser = Transform(
                fail,
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Success.Should().BeFalse();
            result.Parser.Should().BeSameAs(fail);
        }

        [Test]
        public void Parse_Consumed()
        {
            var parser = Transform(
                Any(),
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Value.Should().Be(1);
            result.Consumed.Should().Be(1);

            result = parser.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();
            var parser = Transform(
                failParser,
                c => int.Parse(c.ToString())
            );
            var results = parser.GetChildren().ToList();
            results.Count.Should().Be(1);
            results[0].Should().BeSameAs(failParser);
        }
    }
}
