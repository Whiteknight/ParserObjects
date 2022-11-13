using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class SequentialParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Sequential(s =>
            {
                var first = s.Parse(Any());
                char second = first == 'a' ? s.Parse(Match('b')) : s.Parse(Match('y'));
                var third = s.Parse(Match('c'));

                return $"{first}{second}{third}".ToUpper();
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ABC");
            result.Consumed.Should().Be(3);

            result = parser.Parse("xyc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("XYC");
            result.Consumed.Should().Be(3);
        }

        [Test]
        public void Parse_Parse_Fail()
        {
            var parser = Sequential<object>(s =>
            {
                s.Parse(Fail<bool>());
                return null;
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_TryParse()
        {
            var parser = Sequential(s =>
            {
                var first = s.TryParse(Any());
                var second = s.TryParse(Fail<bool>());
                return first.Value;
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_TryMatch()
        {
            var parser = Sequential(s =>
            {
                var first = s.TryMatch(Any());
                var second = s.TryMatch(Fail<bool>());
                return $"{first.Success}{second.Success}";
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TrueFalse");
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Sequential(s =>
            {
                s.Fail("test");
                return "";
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_Rewind()
        {
            var parser = Sequential(s =>
            {
                var first = s.Parse(Any());
                var second = s.Parse(Any());
                var third = s.Parse(Any());
                var fail = s.Parse(Fail<char>());

                return $"{first}{second}{third}".ToUpper();
            });

            var input = FromString("abc");
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Sequential(s =>
            {
                return "";
            });

            var result = parser.ToBnf();
            result.Should().Contain("(TARGET) := User Function");
        }
    }
}
