using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class EnumerableSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Peek_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
        }

        [Test]
        public void PutBack_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.PutBack(4);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void AsSequence_GetNext_Test()
        {
            var target = new[] { 1, 2, 3 }.AsSequence();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void AsSequence_GetNext_EndValue()
        {
            var target = new[] { 1, 2, 3 }.AsSequence(100);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(100);
        }

        [Test]
        public void AsSequence_GetNext_EndFunc()
        {
            var target = new[] { 1, 2, 3 }.AsSequence(() => 100);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(100);
        }

        [Test]
        public void CurrentLocation_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.CurrentLocation.Column.Should().Be(0);
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
        public void IsAtEnd_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(1);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(2);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(3);
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_Test()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
            cp.Rewind();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Checkpoint_Putbacks()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.PutBack(4);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
            cp.Rewind();
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Checkpoint_PutbacksIgnored()
        {
            var target = new EnumerableSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);

            var cp = target.Checkpoint();
            target.PutBack(10);
            target.PutBack(11);
            cp.Rewind();
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }
    }
}
