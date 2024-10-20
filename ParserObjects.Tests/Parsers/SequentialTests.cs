using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class SequentialTests
{
    public class NoData
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
        public void Parse_TestParse()
        {
            var parser = Sequential(s =>
            {
                var first = s.TestParse(Any());
                var second = s.TestParse(Fail<bool>());
                return $"{first.Success}{second.Success}";
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TrueFalse");
        }

        [Test]
        public void Parse_Match()
        {
            var parser = Sequential(s =>
            {
                var first = s.Match(Any());
                var second = s.Match(Fail());
                return $"{first}{second}";
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TrueFalse");
        }

        [Test]
        public void Parse_Expect()
        {
            var parser = Sequential(s =>
            {
                s.Expect(Any());
                s.Expect(Any());
                return new string(s.GetCapturedInputs());
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ab");
        }

        [Test]
        public void Parse_Expect_Fail()
        {
            var parser = Sequential(s =>
            {
                s.Expect(Any());
                s.Expect(Fail());
                return new string(s.GetCapturedInputs());
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_GetCapturedInputs()
        {
            var parser = Sequential(s =>
            {
                s.Input.GetNext();
                s.Input.GetNext();
                s.Input.GetNext();
                return new string(s.GetCapturedInputs());
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
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
        public void Parse_Fail_CallbackThrows()
        {
            var parser = Sequential<string>(s =>
            {
                throw new Exception("test");
            });

            var result = Try(parser).Parse("");
            result.Success.Should().BeFalse();
            var ex = result.Data.OfType<Exception>().Value;
            ex.Message.Should().Be("test");
        }

        [Test]
        public void Match_Test()
        {
            var parser = Sequential(s =>
            {
                var first = s.Parse(Any());
                char second = first == 'a' ? s.Parse(Match('b')) : s.Parse(Match('y'));
                var third = s.Parse(Match('c'));

                return $"{first}{second}{third}".ToUpper();
            });

            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(3);
        }

        [Test]
        public void Match_Fail_CallbackThrows()
        {
            var parser = Sequential<string>(s =>
            {
                throw new Exception("test");
            });

            var input = FromString("abc");
            var result = Try(parser).Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Fail()
        {
            var parser = Sequential(s =>
            {
                s.Fail("test");
                return "";
            });

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            input.Consumed.Should().Be(0);
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

    public class Data
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Sequential('D', (s, d) =>
            {
                var first = s.Parse(Any());
                char second = first == 'a' ? s.Parse(Match('b')) : s.Parse(Match('y'));
                var third = s.Parse(Match('c'));

                return $"{first}{second}{third}{d}".ToUpper();
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ABCD");
            result.Consumed.Should().Be(3);

            result = parser.Parse("xyc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("XYCD");
            result.Consumed.Should().Be(3);
        }
    }
}
