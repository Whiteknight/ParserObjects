namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public class CalculatorTests
    {
        [Test]
        public void Calculate_Simple()
        {
            var target = new Calculator();
            target.Calculate("1 + 2").Should().Be(3);
            target.Calculate("2 * 3").Should().Be(6);
        }

        [Test]
        public void Calculate_Precidence()
        {
            var target = new Calculator();
            target.Calculate("1 + 2 * 3").Should().Be(7);
        }
    }
}
