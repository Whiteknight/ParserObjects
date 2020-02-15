using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class FilterSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new FilterSequence<int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3, 4, 5, 6 },
                    () => 0
                ),
                x => x % 2 == 0
            );
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var target = new FilterSequence<int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2,  5, 6 },
                    () => 0
                ),
                x => x % 2 == 0
            );
            target.GetNext().Should().Be(2);
            target.PutBack(4);
            target.PutBack(3);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }
    }
}
