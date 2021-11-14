using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class ListSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Peek_Test()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
        }

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

        [Test]
        public void CurrentLocation_Test()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext().Should().Be(1);
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
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(1);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(2);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(3);
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Location_Test()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void Location_Rewind()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            var checkpoint = target.Checkpoint();

            target.GetNext();
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);

            checkpoint.Rewind();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var target = new ListSequence<int>(new[] { 1, 2, 3 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
            cp.Rewind();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }
    }
}
