using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;

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
            result.Should().Contain("parser := (. | <ALREADY SEEN UNNAMED PARSER>)");
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
        public void ToBnf_Deferred()
        {
            var parser = Deferred(() => Any()).Named("parser");
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
        public void ToBnf_Examine()
        {
            var parser = Examine(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
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
            result.Should().Contain("parser := (.)?");
        }

        [Test]
        public void ToBnf_Flatten()
        {
            var parser = Flatten<string, char>(new ProduceParser<char, string>(s => "test")).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
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
            var parser = Any().List(minimum: 5).Named("parser");
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
        public void ToBnf_PositiveLookahead()
        {
            var parser = PositiveLookahead(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?= . )");
        }

        [Test]
        public void ToBnf_Produce()
        {
            var parser = Produce(() => 'a').Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
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
                    (l, op, r) => l + op + r
                )
                .Named("equality");

            var result = equality.ToBnf();
            result.Should().Contain("equals := '='");
            result.Should().Contain("equality := <number> (<equals> <equality>)*");
        }

        [Test]
        public void ToBnf_Rule_2()
        {
            var parser = (Any(), Any()).Produce((a, b) => "").Named("parser");
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
    }
}
