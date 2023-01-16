using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public static class MatchTests
    {
        public class ByIEnumerable
        {
            [Test]
            public void Parse_Test()
            {
                var parser = Match("abc");
                var input = FromString("abcd");
                var result = parser.Parse(input);
                result.Success.Should().BeTrue();
                result.Value.Count.Should().Be(3);
                result.Value[0].Should().Be('a');
                result.Value[1].Should().Be('b');
                result.Value[2].Should().Be('c');
                result.Consumed.Should().Be(3);
            }

            [Test]
            public void Parse_Fail_Start()
            {
                var parser = Match("abc");
                var input = FromString("Xabcd");
                var result = parser.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void Parse_Fail_Second()
            {
                var parser = Match("abc");
                var input = FromString("aXbcd");
                var result = parser.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void Parse_Fail_Third()
            {
                var parser = Match("abc");
                var input = FromString("abXcd");
                var result = parser.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void Parse_Empty()
            {
                var parser = Match("");
                var input = FromString("abc");
                var result = parser.Parse(input);
                result.Success.Should().BeTrue();
                result.Value.Count.Should().Be(0);
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void GetChildren_Test()
            {
                var parser = Match("abc");
                var input = FromString("abcd");
                parser.GetChildren().Count().Should().Be(0);
            }

            [Test]
            public void ToBnf_Test()
            {
                var parser = Match("test").Named("match");
                var result = parser.ToBnf();
                result.Should().Contain("match := 't' 'e' 's' 't'");
            }
        }

        public class ByPredicate
        {
            [Test]
            public void Parse_Test()
            {
                var parser = Match(char.IsNumber);
                var input = FromString("123");
                var result = parser.Parse(input);
                result.Value.Should().Be('1');
                result.Consumed.Should().Be(1);
            }

            [Test]
            public void Parse_Fail()
            {
                var parser = Match(char.IsLetter);
                var input = FromString("123");
                var result = parser.Parse(input);
                result.Success.Should().BeFalse();
                result.Consumed.Should().Be(0);
            }

            [Test]
            public void GetChildren_Test()
            {
                var parser = Match(char.IsNumber);
                parser.GetChildren().Count().Should().Be(0);
            }

            [Test]
            public void MatchEndConsumesZero()
            {
                var parser = Match(c => c == '\0');
                var result = parser.Parse("");
                result.Success.Should().BeTrue();
                result.Consumed.Should().Be(0);
            }
        }
    }
}
