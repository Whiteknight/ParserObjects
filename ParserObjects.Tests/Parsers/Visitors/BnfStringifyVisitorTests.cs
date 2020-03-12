using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers.Visitors
{
    public class BnfStringifyVisitorTests
    {
        [Test]
        public void ToBnf_Any()
        {
            var parser = Any<char>().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_Empty()
        {
            var parser = Empty<char>().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ()");
        }

        [Test]
        public void ToBnf_End()
        {
            var parser = End<char>().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := END");
        }

        [Test]
        public void ToBnf_Fail()
        {
            var parser = Fail<char, object>().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := FAIL");
        }

        [Test]
        public void ToBnf_First_1()
        {
            var parser = First(
                Any<char>()
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_First_2()
        {
            var parser = First(
                Any<char>(),
                Any<char>()
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. | .)");
        }

        [Test]
        public void ToBnf_First_4()
        {
            var parser = First(
                Any<char>(),
                Any<char>(),
                Any<char>(),
                Any<char>()
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. | . | . | .)");
        }

        [Test]
        public void ToBnf_First_2_Optional()
        {
            var parser = First(
                Any<char>(),
                Produce<char, char>(() => '\0')
            ).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (.)?");
        }

        [Test]
        public void ToBnf_List()
        {
            var parser = Any<char>().List().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .*");
        }

        [Test]
        public void ToBnf_List_AtLeastOne()
        {
            var parser = Any<char>().List(true).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .+");
        }

        [Test]
        public void ToBnf_LeftApply()
        {
            var number = Match<char>(char.IsNumber)
                .Transform(c => c.ToString())
                .Named("number");
            var multiply = Match<char>("*")
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
            var add = Match<char>("+")
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
            var parser = Match<char>("test").Named("match");
            var result = parser.ToBnf();
            result.Should().Contain("match := 't' 'e' 's' 't'");
        }

        [Test]
        public void ToBnf_NegativeLookahead()
        {
            var parser = NegativeLookahead(Any<char>()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?! . )");
        }

        [Test]
        public void ToBnf_PositiveLookahead()
        {
            var parser = PositiveLookahead(Any<char>()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (?= . )");
        }

        [Test]
        public void ToBnf_Produce()
        {
            var parser = Produce<char, char>(() => 'a').Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }

        [Test]
        public void ToBnf_RightApply()
        {
            // In C#, assignment ("=") is right-associative
            // 1=2=3 should parse as 1=(2=3)
            var number = Match<char>(char.IsNumber)
                .Transform(c => c.ToString())
                .Named("number");
            var equals = Match<char>("=")
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
            var parser = (Any<char>(), Any<char>()).Produce((a, b) => "").Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := (. .)");
        }

        [Test]
        public void ToBnf_Trie()
        {
            var parser = Trie<char, string>(trie => trie
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
