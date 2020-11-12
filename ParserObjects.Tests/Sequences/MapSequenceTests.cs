using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class MapSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Peek_Test()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.Peek().Should().Be(2);
            target.Peek().Should().Be(2);
            target.Peek().Should().Be(2);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
        }

        [Test]
        public void Select_Test()
        {
            var source = new EnumerableSequence<int>(
                new[] { 1, 2, 3 },
                () => 0
            );
            var target = source.Select(x => x * 2);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Location_Test()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x
            );
            target.GetNext().Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext().Should().Be(2);
            target.CurrentLocation.Column.Should().Be(2);
            target.PutBack(2);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext().Should().Be(2);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext().Should().Be(3);
            target.CurrentLocation.Column.Should().Be(3);
            target.GetNext().Should().Be(0);
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void Putback_Test()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.PutBack(10);
            target.GetNext().Should().Be(10);
            target.GetNext().Should().Be(6);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.GetNext().Should().Be(2);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
            cp.Rewind();
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Checkpoint_Putbacks()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.GetNext().Should().Be(2);
            target.PutBack(12);
            target.PutBack(10);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(10);
            target.GetNext().Should().Be(12);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);

            cp.Rewind();
            target.GetNext().Should().Be(10);
            target.GetNext().Should().Be(12);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Checkpoint_PutbacksIgnored()
        {
            var target = new MapSequence<int, int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x * 2
            );
            target.GetNext().Should().Be(2);

            var cp = target.Checkpoint();
            target.PutBack(12);
            target.PutBack(10);
            cp.Rewind();

            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }
    }
}
