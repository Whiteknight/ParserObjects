using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class FromStringTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GetNext_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Peek_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Peek_End(bool normalize)
        {
            var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.Peek().Should().Be('\0');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetRemainder_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetNext().Should().Be('a');
            target.GetRemainder().Should().Be("bc");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetRemainder_Full(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetRemainder().Should().Be("abc");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetRemainder_Empty(bool normalize)
        {
            var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetRemainder().Should().Be("");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetRemainder_End(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetNext();
            target.GetNext();
            target.GetNext();
            target.GetRemainder().Should().Be("");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetNext_Empty(bool normalize)
        {
            var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            // Every get attempt past the end of the string will return '\0'
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetNext_Empty_CustomEndSentinel(bool normalize)
        {
            var target = FromString("", new SequenceOptions<char>
            {
                EndSentinel = 'X',
                MaintainLineEndings = !normalize
            });
            // Every get attempt past the end of the string will return '\0'
            target.GetNext().Should().Be('X');
            target.GetNext().Should().Be('X');
            target.GetNext().Should().Be('X');
            target.GetNext().Should().Be('X');
        }

        [Test]
        public void GetNext_WindowsNewlines()
        {
            var target = FromString("\r\na\r\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_WindowsNewlines_NonNormalized()
        {
            var target = FromString("\r\na\r\n", new SequenceOptions<char>
            {
                MaintainLineEndings = true
            });
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_UnixNewlines()
        {
            var target = FromString("\na\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_OldMacNewlines()
        {
            var target = FromString("\ra\r");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_OldMacNewlines_NonNormalized()
        {
            var target = FromString("\ra\r", new SequenceOptions<char>
            {
                MaintainLineEndings = true
            });
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_WindowsNewlines()
        {
            var target = FromString("\r\na\r\n");
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
            var target = FromString("\na\n");
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
            var target = FromString("\ra\r");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Location_Test(bool normalize)
        {
            var target = FromString("a\nbc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
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

        [TestCase(true)]
        [TestCase(false)]
        public void Location_Rewind(bool normalize)
        {
            var target = FromString("abc\nde", new SequenceOptions<char> { MaintainLineEndings = !normalize });
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

        [TestCase(true)]
        [TestCase(false)]
        public void Reset_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.Reset();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IsAtEnd_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Checkpoint_Test(bool normalize)
        {
            var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
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
            var target = FromString("\r\n");
            var next = target.GetNext();
            next.Should().Be('\n');
            target.Consumed.Should().Be(1);
        }
    }
}
