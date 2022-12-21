using System.Linq;

namespace ParserObjects.Tests.Sequences;

public static class ToSequenceTests
{
    public class ReadOnlyList
    {
        [Test]
        public void AsSequence_GetNext_Test()
        {
            var target = new[] { 1, 2, 3 }.ToSequence();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void AsSequence_GetNext_EndValue()
        {
            var target = new[] { 1, 2, 3 }.ToSequence(100);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(100);
        }
    }

    public class Enumerable
    {
        [Test]
        public void AsSequence_GetNext_Test()
        {
            var target = new[] { 1, 2, 3 }.Select(x => x).ToSequence();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void AsSequence_GetNext_EndValue()
        {
            var target = new[] { 1, 2, 3 }.Select(x => x).ToSequence(100);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(100);
        }
    }
}
