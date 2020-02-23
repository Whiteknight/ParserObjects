using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class StreamCharacterSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var encoding = Encoding.ASCII;
            var stream = new MemoryStream(encoding.GetBytes("abc"));
            using var target = new StreamCharacterSequence(stream, encoding);
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void Peek_Test()
        {
            var encoding = Encoding.ASCII;
            var stream = new MemoryStream(encoding.GetBytes("abc"));
            using var target = new StreamCharacterSequence(stream, encoding);
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
            target.Peek().Should().Be('a');
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var encoding = Encoding.ASCII;
            var stream = new MemoryStream(encoding.GetBytes("ac"));
            using var target = new StreamCharacterSequence(stream, encoding);
            target.GetNext().Should().Be('a');
            target.PutBack('b');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }

        [Test]
        public void GetNext_UTF8()
        {
            var sc = "abc";
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(sc));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var target = new StreamCharacterSequence(memoryStream, Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');

            memoryStream.Dispose();
            target.Dispose();
        }

        [Test]
        public void PutBack_Test()
        {
            var sc = "abc";
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(sc));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var target = new StreamCharacterSequence(memoryStream, Encoding.UTF8);
            target.GetNext().Should().Be('a');
            target.PutBack('a');
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.PutBack('b');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }
    }
}
