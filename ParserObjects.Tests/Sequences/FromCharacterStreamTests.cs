using System.IO;
using System.Text;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class FromCharacterStreamTests
    {
        private static ISequence<char> GetTarget(string sc, int bufferSize = 32, bool normalizeLineEndings = true, char endSentinel = '\0', Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var memoryStream = new MemoryStream();
            var b = encoding.GetBytes(sc);
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return FromCharacterStream(memoryStream, new SequenceOptions<char>
            {
                BufferSize = bufferSize,
                MaintainLineEndings = !normalizeLineEndings,
                EndSentinel = endSentinel,
                Encoding = encoding
            });
        }

        /* Tests will generally run in several configurations:
         * 1. Whether or not the buffer is large enough to hold the entire data
         * 2. Whether or not we are using ASCII (a single-byte encoding)
         * The FromCharacterStream() method may select different implementations depending on all
         * these possibilities
         */

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_Test(int bufferSize, bool useAscii)
        {
            var target = GetTarget("abc", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_CustomEndSentinel(int bufferSize, bool useAscii)
        {
            var target = GetTarget("abc", endSentinel: 'X', bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('X');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_Test(int bufferSize, bool useAscii)
        {
            var target = GetTarget("abc", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void IsAtEnd_Test(int bufferSize, bool useAscii)
        {
            var target = GetTarget("abc", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_SameBuffer()
        {
            var target = GetTarget("abc", 5);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('a');
            cp.Rewind();
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

        [TestCase(5)]
        [TestCase(2)]
        public void Checkpoint_MultiByteChars(int bufferSize)
        {
            // a is a single byte. Followed by copyright (c) (2 bytes in UTF8), EMDASH (3 bytes)
            // Poop emoji from the astral plane (4 bytes), registered (r) (3 bytes) and then b
            // which is a single byte again.
            // String is approximately similar to "ac-Prb" if you squint real hard, with the buffer
            // break happening between the (r) and b.
            var target = GetTarget("a\u00A9\u2014\U0001F4A9\u00AEb", bufferSize: bufferSize, encoding: Encoding.UTF8);
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('a');
            cp.Rewind();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\u00A9');
            target.GetNext().Should().Be('\u2014');

            // These two are the UTF16 chars for the surrogate pair representing the poop emoji
            target.GetNext().Should().Be('\uD83D');
            target.GetNext().Should().Be('\uDCA9');

            cp = target.Checkpoint();

            target.GetNext().Should().Be('\u00AE');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('\0');

            cp.Rewind();

            target.GetNext().Should().Be('\u00AE');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Checkpoint_MultiByteChars_CannotDivideSurrogates()
        {
            // For stream char sequences we cannot set a checkpoint between the high and low
            // surrogates of a single code point. A string-based char sequence doesn't have this
            // limitation. So, to force a stream-based sequence, make sure to set the buffer size
            // smaller than the length of the string (1 < 2).
            var target = GetTarget("\U0001F4A9", 1);
            target.GetNext().Should().Be('\uD83D');
            Action act = () => target.Checkpoint();
            act.Should().Throw<InvalidOperationException>();
        }

        // There seems to be no way to really test this, because the StreamReader doesn't put a bare
        // high surrogate into the read buffer even if we force it that way inside the stream.
        // [Test]
        // public void GetNext_MalformedInputNoLowSurrogate()
        // {
        //    var memoryStream = new MemoryStream(new byte[] { 0xD8, 0x3D });
        //    memoryStream.Seek(0, SeekOrigin.Begin);
        //    var target = new StreamCharacterSequence(memoryStream, Encoding.UTF8);
        //    Action act = () => target.GetNext();
        //    act.Should().Throw<InvalidOperationException>();
        // }

        [TestCase(true)]
        [TestCase(false)]
        public void Checkpoint_PreviousBuffer(bool useAscii)
        {
            // bufferSize=5 means that we cannot keep all 6 chars in the buffer together.
            // Setting ASCII or not may trigger an optimization in the sequence implementation
            // so we want to test both ways.
            var target = GetTarget("abcdef", bufferSize: 5, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            var cp = target.Checkpoint();

            // Read to the very end of the current buffer then rewind
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');
            target.GetNext().Should().Be('e');
            cp.Rewind();

            // Now read again from the beginning to the first char of the second buffer then rewind
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('f');
            cp.Rewind();

            // Now read the whole thing to completion and see that we get everything.
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('f');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_WindowsNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\r\na\r\n", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_WindowsNewlines_NonNormalized(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\r\na\r\n", normalizeLineEndings: false, bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_UnixNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\na\n", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_OldMacNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\ra\r", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void GetNext_OldMacNewlines_NonNormalized(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\ra\r", normalizeLineEndings: false, bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_WindowsNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\r\na\r\n", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_WindowsNewlines_NonNormalized(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\r\na\r\n", normalizeLineEndings: false, bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_UnixNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\na\n", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_OldMacNewlines(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\ra\r", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Peek_OldMacNewlines_NonNormalized(int bufferSize, bool useAscii)
        {
            var target = GetTarget("\ra\r", normalizeLineEndings: false, bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Location_Test(int bufferSize, bool useAscii)
        {
            var target = GetTarget("a\nbc", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
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

        [TestCase(10, true)]
        [TestCase(10, false)]
        [TestCase(2, true)]
        [TestCase(2, true)]
        public void Location_Rewind(int bufferSize, bool useAscii)
        {
            var target = GetTarget("abc\nde", bufferSize: bufferSize, encoding: useAscii ? Encoding.ASCII : Encoding.UTF8);
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
    }
}
