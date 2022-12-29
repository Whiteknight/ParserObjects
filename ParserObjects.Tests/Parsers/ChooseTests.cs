using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class ChooseTests
{
    public class Extension
    {
        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("aY", false)]
        [TestCase("bX", false)]
        [TestCase("cC", true)]
        [TestCase("dW", false)]
        public void Parse_Extension(string text, bool shouldMatch)
        {
            var parser = Any().Choose(r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");

            var result = parser.Parse(text);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? text.Length : 0);
            if (shouldMatch)
                result.Value.Should().Be(text);
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Fail<object>().Choose(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Choose(c => (IParser<char, string>)null);
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }

    public class Method
    {
        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("aY", false)]
        [TestCase("bX", false)]
        [TestCase("cC", true)]
        [TestCase("dW", false)]
        public void Parse_Test(string text, bool shouldMatch)
        {
            var parser = Choose(Any(), r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");

            var result = parser.Parse(text);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? text.Length : 0);
            if (shouldMatch)
                result.Value.Should().Be(text);
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Choose(Fail<object>(), c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Choose(Any(), c => (IParser<char, string>)null);
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }
}
