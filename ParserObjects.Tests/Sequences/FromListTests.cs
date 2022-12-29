using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public static class FromListTests
{
    public class Integers
    {
        [Test]
        public void GetNext_Test()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);

            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be(2);
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be(3);
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be(0);
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void Peek_Test()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
        }

        [Test]
        public void CurrentLocation_Test()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);
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
            var target = FromList(new[] { 1, 2, 3 }, 0);
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
            var target = FromList(new[] { 1, 2, 3 }, 0);
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
            var target = FromList(new[] { 1, 2, 3 }, 0);
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
            var target = FromList(new[] { 1, 2, 3 }, 0);
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
        public void GetBetween_Test()
        {
            var target = FromList(new[] { 1, 2, 3, 4, 5 }, 0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);
            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be(5);

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result[0].Should().Be(3);
            result[1].Should().Be(4);
        }

        [Test]
        public void Reset_Test()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);

            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be(2);
            target.Consumed.Should().Be(2);

            target.Reset();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);
        }
    }

    public class Chars
    {
        [Test]
        public void GetNext_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });

            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be('c');
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be('\0');
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void Peek_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [Test]
        public void CurrentLocation_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext().Should().Be('c');
            target.CurrentLocation.Column.Should().Be(3);
            target.GetNext().Should().Be('\0');
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be('a');
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be('b');
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be('c');
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Location_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
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
            var target = FromList(new[] { 'a', 'b', 'c' });
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
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
            cp.Rewind();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetBetween_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c', 'd', 'e' });
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');
            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('e');

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result[0].Should().Be('c');
            result[1].Should().Be('d');
        }

        [Test]
        public void Reset_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });

            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.Consumed.Should().Be(2);

            target.Reset();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
        }
    }
}
