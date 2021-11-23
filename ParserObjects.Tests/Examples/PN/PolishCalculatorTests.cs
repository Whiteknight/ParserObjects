namespace ParserObjects.Tests.Examples.PN
{
    public class PolishCalculatorTests
    {
        [Test]
        public void Calculate_SingleValue()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("5");
            result.Should().Be(5);
        }

        [Test]
        public void Calculate_SingleAdd()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("+ 5 6");
            result.Should().Be(11);
        }

        [Test]
        public void Calculate_NestedAdd()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("+ + 1 2 + 3 4");
            result.Should().Be(10);
        }

        [Test]
        public void Calculate_SingleSubtract()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("- 5 6");
            result.Should().Be(-1);
        }

        [Test]
        public void Calculate_SingleMultiply()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("* 5 6");
            result.Should().Be(30);
        }

        [Test]
        public void Calculate_SingleDivide()
        {
            var target = new PolishCalculator();
            var result = target.Calculate("/ 20 5");
            result.Should().Be(4);
        }

        [Test]
        public void Calculate_Mixed()
        {
            // (1 + 2) * (15 / 5) = 9
            var target = new PolishCalculator();
            var result = target.Calculate("* + 1 2 / 15 5");
            result.Should().Be(9);
        }
    }
}
