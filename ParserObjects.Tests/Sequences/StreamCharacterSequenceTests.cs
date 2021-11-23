using System.IO;
using System.Text;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class StreamCharacterSequenceTests
    {
        private static StreamCharacterSequence GetTarget(string sc, int bufferSize = 32, bool normalizeLineEndings = true, char endSentinel = '\0')
        {
            var memoryStream = new MemoryStream();
            var b = Encoding.UTF8.GetBytes(sc);
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new StreamCharacterSequence(memoryStream, Encoding.UTF8, bufferSize: bufferSize, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);
        }

        [Test]
        public void GetNext_Test()
        {
            var target = GetTarget("abc");
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_CustomEndSentinel()
        {
            var target = GetTarget("abc", endSentinel: 'X');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('X');
        }

        [Test]
        public void Peek_Test()
        {
            var target = GetTarget("abc");
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [Test]
        public void GetNext_UTF8()
        {
            var target = GetTarget("abc");
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var target = GetTarget("abc");
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

        [Test]
        public void Checkpoint_MultiByteChars()
        {
            // a is a single byte. Followed by copyright (c) (2 bytes in UTF8), EMDASH (3 bytes)
            // Poop emoji from the astral plane (4 bytes), registered (r) (3 bytes) and then b
            // which is a single byte again.
            // String is approximately similar to "ac-Prb" if you squint real hard, with the buffer
            // break happening between the (r) and b.
            var target = GetTarget("a\u00A9\u2014\U0001F4A9\u00AEb", 5);
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
            // We cannot set a checkpoint between the high and low surrogates of a single codepoint
            var target = GetTarget("\U0001F4A9", 5);
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

        [Test]
        public void Checkpoint_PreviousBuffer()
        {
            var target = GetTarget("abcdef", 5);
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

        [Test]
        public void GetNext_WindowsNewlines()
        {
            var target = GetTarget("\r\na\r\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_WindowsNewlines_NonNormalized()
        {
            var target = GetTarget("\r\na\r\n", normalizeLineEndings: false);
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
            var target = GetTarget("\na\n");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_OldMacNewlines()
        {
            var target = GetTarget("\ra\r");
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\n');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_OldMacNewlines_NonNormalized()
        {
            var target = GetTarget("\ra\r", normalizeLineEndings: false);
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('\r');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_WindowsNewlines()
        {
            var target = GetTarget("\r\na\r\n");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void Peek_WindowsNewlines_NonNormalized()
        {
            var target = GetTarget("\r\na\r\n", normalizeLineEndings: false);
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

        [Test]
        public void Peek_UnixNewlines()
        {
            var target = GetTarget("\na\n");
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
            var target = GetTarget("\ra\r");
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\n');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void Peek_OldMacNewlines_NonNormalized()
        {
            var target = GetTarget("\ra\r", normalizeLineEndings: false);
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('a');
            target.GetNext();
            target.Peek().Should().Be('\r');
            target.GetNext();
            target.Peek().Should().Be('\0');
        }

        [Test]
        public void ToCharSequence_Stream()
        {
            var memoryStream = new MemoryStream();
            var b = Encoding.UTF8.GetBytes("abc");
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var target = memoryStream.ToCharSequence();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void ToCharSequence_StreamReader()
        {
            var memoryStream = new MemoryStream();
            var b = Encoding.UTF8.GetBytes("abc");
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStream);
            var target = reader.ToCharSequence();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void FileStream_Test()
        {
            var fileName = Guid.NewGuid().ToString() + ".txt";
            try
            {
                File.WriteAllText(fileName, "test");
                using (var target = new StreamCharacterSequence(fileName))
                {
                    target.GetNext().Should().Be('t');
                    target.GetNext().Should().Be('e');
                    target.GetNext().Should().Be('s');
                    target.GetNext().Should().Be('t');
                }
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [Test]
        public void Location_Test()
        {
            var target = GetTarget("a\nbc");
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
            var target = GetTarget("abc\nde");
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
