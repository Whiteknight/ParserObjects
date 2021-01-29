using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class StringCharacterSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [Test]
        public void Peek_End()
        {
            var target = new StringCharacterSequence("");
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void AsCharacterSequence_GetNext_Test()
        {
            var target = "abc".ToCharacterSequence();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetRemainder_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.GetRemainder().Should().Be("bc");
        }

        [Test]
        public void GetRemainder_Full()
        {
            var target = new StringCharacterSequence("abc");
            target.GetRemainder().Should().Be("abc");
        }

        [Test]
        public void GetRemainder_Empty()
        {
            var target = new StringCharacterSequence("");
            target.GetRemainder().Should().Be("");
        }

        [Test]
        public void GetRemainder_End()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext();
            target.GetNext();
            target.GetNext();
            target.GetRemainder().Should().Be("");
        }

        [Test]
        public void GetNext_Empty()
        {
            var target = new StringCharacterSequence("");
            // Every get attempt past the end of the string will return '\0'
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_WindowsNewlines()
        {
            var target = new StringCharacterSequence("\r\na\r\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_UnixNewlines()
        {
            var target = new StringCharacterSequence("\na\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_OldMacNewlines()
        {
            var target = new StringCharacterSequence("\ra\r");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_WindowsNewlines()
        {
            var target = new StringCharacterSequence("\r\na\r\n");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void Peek_UnixNewlines()
        {
            var target = new StringCharacterSequence("\na\n");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void Peek_OldMacNewlines()
        {
            var target = new StringCharacterSequence("\ra\r");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void Location_Test()
        {
            var target = new StringCharacterSequence("a\nbc");
            target.GetNext().Should().Be('a');
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);

            target.GetNext().Should().Be('\n');

            target.GetNext().Should().Be('b');
            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(1);

            target.GetNext().Should().Be('c');
            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(2);

            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Location_Rewind()
        {
            var target = new StringCharacterSequence("abc\nde");
            target.GetNext();
            target.GetNext();
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);
            var checkpoint = target.Checkpoint();

            target.GetNext().Should().Be('\n');
            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(0);

            checkpoint.Rewind();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void Reset_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.Reset();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_Test()
        {
            var target = new StringCharacterSequence("abc");
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
            cp.Rewind();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void ReadNewlineIncrementsConsumed()
        {
            var target = new StringCharacterSequence("\r\n");
            var next = target.GetNext();
            next.Should().Be('\n');
            target.Consumed.Should().Be(1);
        }
    }
}
