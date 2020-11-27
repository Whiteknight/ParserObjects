using FluentAssertions;
using NUnit.Framework;

namespace ParserObjects.Tests.Examples.Parens
{
    public class ExpressionParenthesizerTests
    {
        [Test]
        public void InfixOperators_Test()
        {
            var result = ExpressionParenthesizer.Parenthesize("1+2*3-4/5");
            result.Should().Be("((1+(2*3))-(4/5))");
        }

        [Test]
        public void PrefixAndPostfix_Test()
        {
            var result = ExpressionParenthesizer.Parenthesize("-1!");
            result.Should().Be("(-(1!))");
        }

        [Test]
        public void ImpliedMultiplication_Test()
        {
            var result = ExpressionParenthesizer.Parenthesize("6/2(2+1)");
            result.Should().Be("(6/(2*((2+1))))");
        }
    }
}
