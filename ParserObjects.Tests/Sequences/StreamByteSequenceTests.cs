using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class StreamByteSequenceTests
    {
        private static StreamByteSequence GetTarget(params byte[] b)
        {
            var memoryStream = new MemoryStream();
            memoryStream.Write(b);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new StreamByteSequence(memoryStream);
        }

        [Test]
        public void GetNext_Test()
        {
            var target = GetTarget(1, 2, 3);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Peek_Test()
        {
            var target = GetTarget(1, 2, 3);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
            target.Peek().Should().Be(1);
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var target = GetTarget(1, 3);
            target.GetNext().Should().Be(1);
            target.PutBack(2);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void PutBack_Test()
        {
            var target = GetTarget(1, 2, 3);
            target.GetNext().Should().Be(1);
            target.PutBack(1);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.PutBack(2);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void ToByteSequence_Test()
        {
            var memoryStream = new MemoryStream();
            memoryStream.Write(new byte[] { 1, 2, 3, });
            memoryStream.Seek(0, SeekOrigin.Begin);
            var target = memoryStream.ToByteSequence();

            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void FileStream_Test()
        {
            var fileName = Guid.NewGuid().ToString() + ".txt";
            try
            {
                File.WriteAllText(fileName, "test");
                using (var target = new StreamByteSequence(fileName))
                {
                    target.GetNext().Should().Be((byte)'t');
                    target.GetNext().Should().Be((byte)'e');
                    target.GetNext().Should().Be((byte)'s');
                    target.GetNext().Should().Be((byte)'t');
                }
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }

        }
    }
}
