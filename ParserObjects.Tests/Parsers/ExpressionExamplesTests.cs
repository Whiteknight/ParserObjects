using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class ExpressionExamplesTests
    {
        private abstract class ParseNode
        {
        }

        private class InfixExpressionParseNode : ParseNode
        {
            public ParseNode Left { get; set; }
            public string Operator { get; set; }
            public ParseNode Right { get; set; }
        }

        private class NumberValueParseNode : ParseNode
        {
            public string Value { get; set; }
        }

        private class PrefixExpressionParseNode : ParseNode
        {
            public string Operator { get; set; }
            public ParseNode Right { get; set; }
        }

        [Test]
        public void Parse_2Precedences_LeftAssociative()
        {
            var number = Match<char>(char.IsNumber).Transform(c => (ParseNode)new NumberValueParseNode { Value = c.ToString() });
            var multiply = Match<char>("*").Transform(c => c[0].ToString());
            var multiplicative = LeftApply(
                number,
                left => Rule(
                    left,
                    multiply,
                    number,
                    (l, op, r) => (ParseNode) new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
                )
            );
            var add = Match<char>("+").Transform(c => c[0].ToString());
            var additive = LeftApply(
                multiplicative,
                left => Rule(
                    left,
                    add,
                    multiplicative,
                    (l, op, r) => (ParseNode) new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
                )
            );

            // 1+2*3 should parse as 1+(2*3)
            var result1 = additive.Parse("1+2*3").Value as InfixExpressionParseNode;
            (result1.Left as NumberValueParseNode).Value.Should().Be("1");
            result1.Operator.Should().Be("+");
            var rhs1 = result1.Right as InfixExpressionParseNode;
            (rhs1.Left as NumberValueParseNode).Value.Should().Be("2");
            rhs1.Operator.Should().Be("*");
            (rhs1.Right as NumberValueParseNode).Value.Should().Be("3");

            // 1*2+3 should parse as (1*2)+3
            var result2 = additive.Parse("1*2+3").Value as InfixExpressionParseNode;
            var lhs2 = result2.Left as InfixExpressionParseNode;
            (lhs2.Left as NumberValueParseNode).Value.Should().Be("1");
            lhs2.Operator.Should().Be("*");
            (lhs2.Right as NumberValueParseNode).Value.Should().Be("2");
            result2.Operator.Should().Be("+");
            (result2.Right as NumberValueParseNode).Value.Should().Be("3");
        }

        [Test]
        public void Parse_RightAssociative()
        {
            // In C#, assignment ("=") is right-associative
            // 1=2=3 should parse as 1=(2=3)
            var number = Match<char>(char.IsNumber).Transform(c => (ParseNode)new NumberValueParseNode { Value = c.ToString() });
            var equals = Match<char>("=").Transform(c => c[0].ToString());
            var equality = RightApply(
                number,
                equals,
                (l, op, r) => (ParseNode) new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
            );
            var result1 = equality.Parse("1=2=3").Value as InfixExpressionParseNode;
            (result1.Left as NumberValueParseNode).Value.Should().Be("1");
            result1.Operator.Should().Be("=");
            var rhs1 = result1.Right as InfixExpressionParseNode;
            (rhs1.Left as NumberValueParseNode).Value.Should().Be("2");
            rhs1.Operator.Should().Be("=");
            (rhs1.Right as NumberValueParseNode).Value.Should().Be("3");
        }

        [Test]
        public void Parse_RightAssociative_Recursive()
        {
            // In C#, assignment ("=") is right-associative
            // 1=2=3 should parse as 1=(2=3)
            // This is the recursive version of the above with Deferred() instead.
            var number = Match<char>(char.IsNumber).Transform(c => (ParseNode)new NumberValueParseNode { Value = c.ToString() });
            var equals = Match<char>("=").Transform(c => c[0].ToString());
            IParser<char, ParseNode> equalityCore = null;
            var equality = Deferred(() => equalityCore);
            equalityCore = First(
                Rule(
                    number,
                    equals,
                    equality,
                    (l, op, r) => (ParseNode) new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
                ),
                number
            );

            var result1 = equality.Parse("1=2=3").Value as InfixExpressionParseNode;
            (result1.Left as NumberValueParseNode).Value.Should().Be("1");
            result1.Operator.Should().Be("=");
            var rhs1 = result1.Right as InfixExpressionParseNode;
            (rhs1.Left as NumberValueParseNode).Value.Should().Be("2");
            rhs1.Operator.Should().Be("=");
            (rhs1.Right as NumberValueParseNode).Value.Should().Be("3");
        }

        [Test]
        public void Parse_PrefixAndInfix_Minus()
        {
            var number = Match<char>(char.IsNumber).Transform(c => (ParseNode)new NumberValueParseNode { Value = c.ToString() });
            var minus = Match<char>("-").Transform(c => c[0].ToString());
            var maybeNegative = First(
                Rule(
                    minus,
                    number,
                    (m, n) => (ParseNode) new PrefixExpressionParseNode { Operator = m, Right = n }
                ),
                number
            );
            var additive = LeftApply(
                maybeNegative,
                left => Rule(
                    left,
                    minus,
                    maybeNegative,
                    (l, op, r) => (ParseNode)new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
                )
            );

            // -1--2 should parse as (-1)-(-2)
            var result1 = additive.Parse("-1--2").Value as InfixExpressionParseNode;
            var lhs = result1.Left as PrefixExpressionParseNode;
            lhs.Operator.Should().Be("-");
            (lhs.Right as NumberValueParseNode).Value.Should().Be("1");

            result1.Operator.Should().Be("-");

            var rhs = result1.Right as PrefixExpressionParseNode;
            rhs.Operator.Should().Be("-");
            (rhs.Right as NumberValueParseNode).Value.Should().Be("2");
        }

        [Test]
        public void Replace_OperatorParser()
        {
            // Show how we can replace one parser in the tree with another parser.
            // replace the "add" parser with a "subtract" parser we define later, and
            // use the new parser tree to parse a modified grammar
            var number = Match<char>(char.IsNumber).Transform(c => (ParseNode)new NumberValueParseNode { Value = c.ToString() });
            var add = Match<char>("+")
                .Transform(c => c[0].ToString())
                .Replaceable()
                .Named("add");
            var additive = LeftApply(
                number,
                left => Rule(
                    left,
                    add,
                    number,
                    (l, op, r) => (ParseNode)new InfixExpressionParseNode { Left = l, Operator = op, Right = r }
                )
            );
            var expr = Rule(
                additive,
                End<char>(),
                (a, eoi) => a
            );

            var subtract = Match<char>("-")
                .Transform(c => c[0].ToString());
            expr.Replace("add", subtract);

            // Show that we can parse the new grammar with '-' 
            var result1 = expr.Parse("1-2").Value as InfixExpressionParseNode;
            (result1.Left as NumberValueParseNode).Value.Should().Be("1");
            result1.Operator.Should().Be("-");
            (result1.Right as NumberValueParseNode).Value.Should().Be("2");

            // Show that we can't parse the old grammar with '+'
            var result2 = expr.Parse("1+2");
            result2.Success.Should().BeFalse();
        }
    }
}
