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
        [TestCase("4 + -5 - 6", -7)]
        [TestCase("5! - 100", 20)]
        public void Calculate(string expr, int result)
        {
            var target = new Calculator();
            target.Calculate(expr).Should().Be(result);
        }
    }
}
