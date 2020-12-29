using FluentAssertions;
using NUnit.Framework;

namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public class CalculatorTests
    {
        [TestCase("1 + 2", 3)]
        [TestCase("2 * 3", 6)]
        [TestCase("2^2^2", 16)]
        [TestCase("1 + 2 * 3", 7)]
        [TestCase("(1 + 2) * 3", 9)]
        [TestCase("1 + -2", -1)]
        public void Calculate(string expr, int result)
        {
            var target = new Calculator();
            target.Calculate(expr).Should().Be(result);
        }
    }
}