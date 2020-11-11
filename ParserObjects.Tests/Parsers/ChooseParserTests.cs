using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChooseParserTests
    {
        [Test]
        public void Parse_Basic()
        {
            var parser = Any().Choose(c =>
            {
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aX");

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("bY");

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();

            result = parser.Parse("cC");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("cC");
        }

        [Test]
        public void Parse_EquivalentToPositiveLookaheadChain()
        {
            var parser1 = Match('a').Choose(a => Match(a + "bc"));
            var parser2 = PositiveLookahead(Match('a')).Chain(a => Match(a + "bc"));

            var result1 = parser1.Parse("abc");
            result1.Success.Should().BeTrue();
            result1.Value.Should().BeEquivalentTo('a', 'b', 'c');

            var result2 = parser1.Parse("abc");
            result2.Success.Should().BeTrue();
            result2.Value.Should().BeEquivalentTo('a', 'b', 'c');
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Fail<object>().Choose(c => Any());
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Choose(c => (IParser<char, string>)null);
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild()
        {
            var initial = Fail<char>();
            var parser = initial.Choose(c => Any());
            parser = parser.ReplaceChild(initial, Any()) as IParser<char, char>;
            var result = parser.Parse("ab");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }
}
