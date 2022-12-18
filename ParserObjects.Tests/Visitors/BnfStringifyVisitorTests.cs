using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Bnf;
using ParserObjects.Internal.Utility;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors
{
    public class BnfStringifyVisitorTests
    {
        [Test]
        public void ToBnf_AlreadySeenUnnamedParser_RecurseOk()
        {
            var offender = Any();
            var parser = (offender, offender).First().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. | .)");
        }

        [Test]
        public void ToBnf_AlreadySeenUnnamedParser_RecurseFail()
        {
            var offender = Deferred(() => Any());
            var parser = (offender, offender).First().Named("parser");
            var result = parser.ToBnf();
            var offenderString = offender.ToString();
            result.Should().Contain($"parser := (. | <ALREADY SEEN {offenderString}>)");
        }

        [Test]
        public void ToBnf_And()
        {
            var parser = And(Any(), Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := . && .");
        }

        [Test]
        public void ToBnf_Any()
        {
            var parser = Any().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_Chain()
        {
            var parser = Any().Chain(c => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .->Chain");
        }

        [Test]
        public void ToBnf_Chain_Untyped()
        {
            var parser = Empty().Chain(c => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ()->Chain");
        }

        [Test]
        public void ToBnf_Choose()
        {
            var parser = Any().Choose(c => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?=.)->Chain");
        }

        [Test]
        public void ToBnf_Context()
        {
            var parser = DataContext(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_Empty()
        {
            var parser = Empty().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ()");
        }

        [Test]
        public void ToBnf_End()
        {
            var parser = End().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := END");
        }

        [Test]
        public void ToBnf_Fail()
        {
            var parser = Fail<object>().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := FAIL");
        }

        [Test]
        public void ToBnf_First_1()
        {
            var parser = First(
                Any()
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_First_2()
        {
            var parser = First(
                Any(),
                Any()
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. | .)");
        }

        [Test]
        public void ToBnf_First_4()
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
        public void ToBnf_First_2_Optional()
        {
            var parser = First(
                Any(),
                Produce(() => '\0')
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. | PRODUCE)");
        }

        [Test]
        public void ToBnf_Func()
        {
            var parser = Function<string>(args => args.Success("")).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := User Function");
        }

        [Test]
        public void ToBnf_If()
        {
            var parser = If(End(), Any(), Peek()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := IF END THEN . ELSE (?=.)");
        }

        [Test]
        public void ToBnf_LimitedList()
        {
            var parser = Any().List().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .*");
        }

        [Test]
        public void ToBnf_LimitedList_AtLeastOne()
        {
            var parser = Any().List(true).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .+");
        }

        [Test]
        public void ToBnf_LimitedList_Max()
        {
            var parser = Any().List(maximum: 5).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .{0, 5}");
        }

        [Test]
        public void ToBnf_LimitedList_Min()
        {
            var parser = Any().List(5).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .{5,}");
        }

        [Test]
        public void ToBnf_LeftApply()
        {
            var number = Match(char.IsNumber)
                .Transform(c => c.ToString())
                .Named("number");
            var multiply = Match("*")
                .Transform(c => c[0].ToString())
                .Named("asterisk");
            var multiplicative = LeftApply(
                    number,
                    left => Rule(
                        left,
                        multiply,
                        number,
                        (l, op, r) => l + op + r
                    )
                )
                .Named("multiplicative");
            var add = Match("+")
                .Transform(c => c[0].ToString())
                .Named("plus");
            var additive = LeftApply(
                    multiplicative,
                    left => Rule(
                        left,
                        add,
                        multiplicative,
                        (l, op, r) => l + op + r
                    )
                )
                .Named("additive");

            var result = additive.ToBnf();

            // I don't like to over-specify the exact format of the string, but we need something
            result.Should().Contain("plus := '+'");
            result.Should().Contain("asterisk := '*'");
            result.Should().Contain("multiplicative := (<multiplicative> <asterisk> <number>) | <number>");
            result.Should().Contain("additive := (<additive> <plus> <multiplicative>) | <multiplicative>");
        }

        [Test]
        public void ToBnf_MatchSequence()
        {
            var parser = Match("test").Named("match");
            var result = parser.ToBnf();
            result.Should().Contain("match := 't' 'e' 's' 't'");
        }

        [Test]
        public void ToBnf_NegativeLookahead()
        {
            var parser = NegativeLookahead(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?! . )");
        }

        [Test]
        public void ToBnf_None_Output()
        {
            var parser = Any().None().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?=.)");
        }

        [Test]
        public void ToBnf_None_NoOutput()
        {
            var parser = Empty().None().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?=())");
        }

        [Test]
        public void ToBnf_Not()
        {
            var parser = Not(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := !.");
        }

        [Test]
        public void ToBnf_Or()
        {
            var parser = Or(Any(), Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := . || .");
        }

        [Test]
        public void ToBnf_Peek()
        {
            var parser = Peek().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?=.)");
        }

        [Test]
        public void ToBnf_PositiveLookahead()
        {
            var parser = PositiveLookahead(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?= . )");
        }

        [Test]
        public void ToBnf_Pratt()
        {
            var parser = Pratt<char>(c => c
                .Add(Match('+'))
                .Add(Match('-'))
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().StartWith("parser := PRATT(MATCH, MATCH);");
        }

        [Test]
        public void ToBnf_Produce()
        {
            var parser = Produce(() => 'a').Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }

        [Test]
        public void ToBnf_Regex()
        {
            var parser = Regex("(a|b)c*d").Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := /(a|b)c*d/");
        }

        [Test]
        public void ToBnf_RightApply()
        {
            // In C#, assignment ("=") is right-associative
            // 1=2=3 should parse as 1=(2=3)
            var number = Match(char.IsNumber)
                .Transform(c => c.ToString())
                .Named("number");
            var equals = Match("=")
                .Transform(c => c[0].ToString())
                .Named("equals");
            var equality = RightApply(
                    number,
                    equals,
                    args => args.Left + args.Middle + args.Right
                )
                .Named("equality");

            var result = equality.ToBnf();
            result.Should().Contain("equals := '='");
            result.Should().Contain("equality := <number> (<equals> <equality>)*");
        }

        [Test]
        public void ToBnf_Rule_2()
        {
            var parser = (Any(), Any()).Rule((a, b) => "").Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. .)");
        }

        [Test]
        public void ToBnf_Trie()
        {
            var parser = Trie<string>(trie => trie
                    .Add("abc")
                    .Add("abd")
                    .Add("xyz")
                )
                .Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ('a' 'b' 'c') | ('a' 'b' 'd') | ('x' 'y' 'z')");
        }

        private class TestParser : IParser<char, string>
        {
            public int Id { get; } = UniqueIntegerGenerator.GetNext();

            public string Name => null;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public bool Match(IParseState<char> state)
            {
                throw new NotImplementedException();
            }

            public IResult<string> Parse(IParseState<char> state)
            {
                throw new NotImplementedException();
            }

            public INamed SetName(string name)
            {
                throw new NotImplementedException();
            }

            IResult IParser<char>.Parse(IParseState<char> state)
            {
                throw new NotImplementedException();
            }
        }

        private class TestParserBnfVisitor : IPartialVisitor<BnfStringifyVisitor>
        {
            public bool TryAccept(IParser parser, BnfStringifyVisitor state)
            {
                if (parser is TestParser tp)
                {
                    state.Append("TEST");
                    return true;
                }

                return false;
            }
        }

        [Test]
        public void CustomParserTest()
        {
            var stringifier = new BnfStringifier();
            stringifier.Add<TestParserBnfVisitor>();

            var parser = List(new TestParser()).Named("TARGET");

            var result = stringifier.Stringify(parser);
            result.Should().Contain("TARGET := TEST*");
        }
    }
}
