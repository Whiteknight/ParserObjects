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
        public void GetNext_Flags()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);

            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be(2);
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be(3);
            target.Consumed.Should().Be(3);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
            target.GetNext().Should().Be(0);
            target.Consumed.Should().Be(3);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
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
        public void Peek_DoesNotChangeFlags()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Peek().Should().Be(1);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Peek().Should().Be(1);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
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
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be(1);
            target.IsAtEnd.Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be(2);
            target.IsAtEnd.Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be(3);
            target.IsAtEnd.Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
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
        public void Checkpoint_Beginning()
        {
            var target = FromList(new[] { 1, 2, 3 }, 0);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
            target.GetNext().Should().Be(0);
            cp.Rewind();
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be(1);
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

            var result = target.GetArrayBetween(cp1, cp2);
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

            var stats = target.GetStatistics();
            stats.ItemsRead.Should().Be(0);

            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be('c');
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be('\0');
            target.Consumed.Should().Be(3);

            stats = target.GetStatistics();
            stats.ItemsRead.Should().Be(3);
        }

        [Test]
        public void GetNext_Flags()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });

            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be('c');
            target.Consumed.Should().Be(3);
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
            target.GetNext().Should().Be('\0');
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void GetNext_NormalizeLineEndings_Windows()
        {
            var target = FromList(new[] { 'a', '\r', '\n', 'b' });

            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_NormalizeLineEndings_Linux()
        {
            var target = FromList(new[] { 'a', '\n', 'b' });

            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('a');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeFalse();
            target.GetNext().Should().Be('\n');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('b');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeFalse();
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_NormalizeLineEndings_OldMac()
        {
            var target = FromList(new[] { 'a', '\r', 'b', '\r' });

            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('a');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeFalse();
            target.GetNext().Should().Be('\n');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('b');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeFalse();
            target.GetNext().Should().Be('\n');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_NonNormalizedLineEndings()
        {
            var target = FromList(new[] { 'a', '\r', '\n', 'b' }, new SequenceOptions<char>
            {
                MaintainLineEndings = true
            });

            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\n');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeTrue();
            target.GetNext().Should().Be('b');
            target.Flags.Has(SequencePositionFlags.StartOfLine).Should().BeFalse();
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_Empty()
        {
            var target = FromList(new char[0]);

            target.IsAtEnd.Should().BeTrue();
            target.GetNext().Should().Be('\0');
        }

        [TestCase('a', 'a')]
        [TestCase('\r', '\n')]
        [TestCase('\n', '\n')]
        public void GetNext_SingleChar(char input, char output)
        {
            var target = FromList(new[] { input });

            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(output);
            target.IsAtEnd.Should().BeTrue();
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
        public void Peek_DoesNotChangeFlags()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Peek().Should().Be('a');
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Peek().Should().Be('a');
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
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
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('a');
            target.IsAtEnd.Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('b');
            target.IsAtEnd.Should().BeFalse();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('c');
            target.IsAtEnd.Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeTrue();
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
        public void Checkpoint_Beginning()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
            cp.Rewind();
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
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

            var result = target.GetArrayBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result[0].Should().Be('c');
            result[1].Should().Be('d');
        }

        [Test]
        public void Reset_Test()
        {
            var target = FromList(new[] { 'a', 'b', 'c' });

            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');
            target.Consumed.Should().Be(2);

            target.Reset();
            target.Flags.Has(SequencePositionFlags.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequencePositionFlags.EndOfInput).Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');
        }
    }
}
