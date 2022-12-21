using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class AndTests
{
    private static readonly IParser<char, char> _anyParser = Any();
    private static readonly IParser<char, char> _failParser = Fail<char>();

    public class Method
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Parse_Test(bool success1, bool success2)
        {
            var p1 = success1 ? _anyParser : _failParser;
            var p2 = success2 ? _anyParser : _failParser;
            var target = And(p1, p2);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().Be(success2 && success1);
            result.Consumed.Should().Be(success1 && success2 ? 2 : 0);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Match_Test(bool success1, bool success2)
        {
            var p1 = success1 ? _anyParser : _failParser;
            var p2 = success2 ? _anyParser : _failParser;
            var target = And(p1, p2);
            var input = FromString("abc");
            var result = target.Match(input);
            result.Should().Be(success2 && success1);
            input.Consumed.Should().Be(success1 && success2 ? 2 : 0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = And(_anyParser, _failParser);
            var children = target.GetChildren();
            children.Should().Contain(_anyParser);
            children.Should().Contain(_failParser);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = And(_anyParser, _failParser).Named("TARGET");
            var result = target.ToBnf();
            result.Should().Contain("TARGET := . && FAIL");
        }
    }

    public class Extension
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Parse_Test(bool success1, bool success2)
        {
            var p1 = success1 ? _anyParser : _failParser;
            var p2 = success2 ? _anyParser : _failParser;
            var target = p1.And(p2);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().Be(success2 && success1);
            result.Consumed.Should().Be(success1 && success2 ? 2 : 0);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Match_Test(bool success1, bool success2)
        {
            var p1 = success1 ? _anyParser : _failParser;
            var p2 = success2 ? _anyParser : _failParser;
            var target = p1.And(p2);
            var input = FromString("abc");
            var result = target.Match(input);
            result.Should().Be(success2 && success1);
            input.Consumed.Should().Be(success1 && success2 ? 2 : 0);
        }
    }
}
