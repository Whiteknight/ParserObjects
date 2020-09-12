using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Utility;

namespace ParserObjects.Tests.Utility
{
    public class AlwaysFullRingBufferTests
    {
        [Test]
        public void MoveForward_Rollover()
        {
            var target = new AlwaysFullRingBuffer<int>(5, 0, int.MaxValue - 1);
            target.Add(1);
            target.Add(2);
            target.Add(3);

            target.MoveBack();
            target.GetCurrent().Should().Be(2);

            target.MoveBack();
            target.GetCurrent().Should().Be(1);
        }

        [Test]
        public void MoveBack_Rollover()
        {
            var target = new AlwaysFullRingBuffer<int>(5, 0, 0);
            target.MoveBack();
            target.MoveBack();
            target.Add(1);
            target.GetCurrent().Should().Be(1);
        }
    }
}
