using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

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
                char second;
                if (first == 'a')
                    second = s.Parse(Match('b'));
                else
                    second = s.Parse(Match('y'));
                var third = s.Parse(Match('c'));

                return $"{first}{second}{third}".ToUpper();
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ABC");

            result = parser.Parse("xyc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("XYC");
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Sequential<object>(s =>
            {
                s.Parse(Fail<bool>());
                return null;
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
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

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }
    }
}
