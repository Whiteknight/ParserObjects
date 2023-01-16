using System.Linq;
using static ParserObjects.Parsers<char>;

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
            [TestCase("a", true)]
            [TestCase("X", true)]
            [TestCase("1", true)]
            [TestCase("Z", false)]
            public void Parse_Test(string test, bool shouldMatch)
            {
                var parser = First(
                    Match('a'),
                    Match('X'),
                    Match('1')
                );

                var result = parser.Parse(test);
                result.Success.Should().Be(shouldMatch);
                result.Consumed.Should().Be(shouldMatch ? 1 : 0);
            }

            [TestCase("a", true)]
            [TestCase("X", true)]
            [TestCase("1", true)]
            [TestCase("Z", false)]
            public void Parse_Array(string test, bool shouldMatch)
            {
                var parser = First(new[]
                {
                    Match('a'),
                    Match('X'),
                    Match('1')
                });

                var result = parser.Parse(test);
                result.Success.Should().Be(shouldMatch);
                result.Consumed.Should().Be(shouldMatch ? 1 : 0);
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

            [Test]
            public void ToBnf_1()
            {
                var parser = First(
                    Any()
                ).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := .");
            }

            [Test]
            public void ToBnf_2()
            {
                var parser = First(
                    Any(),
                    Any()
                ).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := (. | .)");
            }

            [Test]
            public void ToBnf_4()
            {
                var parser = First(
                    Any(),
                    Any(),
                    Any(),
                    Any()
                ).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := (. | . | . | .)");
            }

            [Test]
            public void ToBnf_2_Optional()
            {
                var parser = First(
                    Any(),
                    Produce(() => '\0')
                ).Named("parser");
                var result = parser.ToBnf();
                result.Should().Contain("parser := (. | PRODUCE)");
            }
        }

        public class OnValueTuple
        {
            private static readonly IParser<char, char> _a = Match('a');
            private static readonly IParser<char, char> _b = Match('b');
            private static readonly IParser<char, char> _c = Match('c');
            private static readonly IParser<char, char> _d = Match('d');
            private static readonly IParser<char, char> _e = Match('e');
            private static readonly IParser<char, char> _f = Match('f');
            private static readonly IParser<char, char> _g = Match('g');
            private static readonly IParser<char, char> _h = Match('h');
            private static readonly IParser<char, char> _i = Match('i');

            private static void RunTest(IParser<char, char> target, string test, bool shouldMatch)
            {
                var result = target.Parse(test);
                result.Success.Should().Be(shouldMatch);
                result.Consumed.Should().Be(shouldMatch ? 1 : 0);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_2(string test, bool shouldMatch)
            {
                var target = (_a, _b).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_3(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_4(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("e", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_5(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d, _e).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("e", true)]
            [TestCase("f", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_6(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d, _e, _f).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("e", true)]
            [TestCase("f", true)]
            [TestCase("g", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_7(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d, _e, _f, _g).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("e", true)]
            [TestCase("f", true)]
            [TestCase("g", true)]
            [TestCase("h", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_8(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d, _e, _f, _g, _h).First();
                RunTest(target, test, shouldMatch);
            }

            [TestCase("a", true)]
            [TestCase("b", true)]
            [TestCase("c", true)]
            [TestCase("d", true)]
            [TestCase("e", true)]
            [TestCase("f", true)]
            [TestCase("g", true)]
            [TestCase("h", true)]
            [TestCase("i", true)]
            [TestCase("Z", false)]
            public void ValueTyple_First_9(string test, bool shouldMatch)
            {
                var target = (_a, _b, _c, _d, _e, _f, _g, _h, _i).First();
                RunTest(target, test, shouldMatch);
            }
        }
    }
}
