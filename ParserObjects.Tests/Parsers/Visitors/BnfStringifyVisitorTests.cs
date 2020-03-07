using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers.Visitors
{
    public class BnfStringifyVisitorTests
    {
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
    }
}
