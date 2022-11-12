using System.Linq;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public static class FirstTests
    {
        public class OnMulti
        {
            [Test]
            public void Parse_Test()
            {
                var target = ProduceMulti(() => "a").First();
                var result = target.Parse("");
                result.Success.Should().BeTrue();
                result.Value.Should().Be('a');
            }

            [Test]
            public void Parse_Test_Predicate()
            {
                var target = ProduceMulti(() => "a").First(r => r.Success);
                var result = target.Parse("");
                result.Success.Should().BeTrue();
                result.Value.Should().Be('a');
            }

            [Test]
            public void Parse_FailNoResults()
            {
                var target = ProduceMulti(() => "").First();
                var result = target.Parse("");
                result.Success.Should().BeFalse();
            }

            [Test]
            public void Parse_FailNoResults_Predicate()
            {
                var target = ProduceMulti(() => "").First(r => r.Success);
                var result = target.Parse("");
                result.Success.Should().BeFalse();
            }

            [Test]
            public void Parse_FailTooMany()
            {
                var target = ProduceMulti(() => "abc").First();
                var result = target.Parse("");
                result.Success.Should().BeTrue();
                result.Value.Should().Be('a');
            }

            [Test]
            public void Parse_FailTooMany_Predicate()
            {
                var target = ProduceMulti(() => "abc").First(r => r.Success);
                var result = target.Parse("");
                result.Success.Should().BeTrue();
                result.Value.Should().Be('a');
            }
        }

        public class Basic
        {
            [Test]
            public void Parse_Test()
            {
                var parser = new FirstParser<char, char>(new[]
                {
                    Match('a'),
                    Match('X'),
                    Match('1')
                });

                parser.CanMatch("a").Should().BeTrue();
                parser.CanMatch("X").Should().BeTrue();
                parser.CanMatch("1").Should().BeTrue();

                parser.CanMatch("b").Should().BeFalse();
            }

            [Test]
            public void GetChildren_Test()
            {
                var aParser = Match('a');
                var xParser = Match('X');
                var oneParser = Match('1');
                var bParser = Match('b');
                var parser = First(
                    aParser,
                    xParser,
                    oneParser
                );
                var results = parser.GetChildren().ToList();
                results.Count.Should().Be(3);
                results[0].Should().Be(aParser);
                results[1].Should().Be(xParser);
                results[2].Should().Be(oneParser);
            }
        }

        public class OnValueTuple
        {
            // TODO: Need to make sure we can match against all options
            private IParser<char, char> _a = Match('a');

            private IParser<char, char> _b = Match('b');
            private IParser<char, char> _c = Match('c');
            private IParser<char, char> _d = Match('d');
            private IParser<char, char> _e = Match('e');
            private IParser<char, char> _f = Match('f');
            private IParser<char, char> _g = Match('g');
            private IParser<char, char> _h = Match('h');
            private IParser<char, char> _i = Match('i');

            [Test]
            public void ValueTyple_First_2()
            {
                var target = (_b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_3()
            {
                var target = (_c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_4()
            {
                var target = (_d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_5()
            {
                var target = (_e, _d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_6()
            {
                var target = (_f, _e, _d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_7()
            {
                var target = (_g, _f, _e, _d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_8()
            {
                var target = (_h, _g, _f, _e, _d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }

            [Test]
            public void ValueTyple_First_9()
            {
                var target = (_i, _h, _g, _f, _e, _d, _c, _b, _a).First();
                target.CanMatch("a").Should().BeTrue();
            }
        }
    }
}
