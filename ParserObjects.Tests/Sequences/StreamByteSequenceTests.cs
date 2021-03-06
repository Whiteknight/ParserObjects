﻿using System;
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
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new StreamByteSequence(memoryStream, bufferSize: 5);
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
        public void IsAtEnd_Test()
        {
            var target = GetTarget(1, 2, 3);
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
            var target = GetTarget(1, 2, 3, 4, 5, 6);
            target.GetNext().Should().Be(1);
            var cp = target.Checkpoint();

            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            cp.Rewind();

            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(5);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void ToByteSequence_Test()
        {
            var memoryStream = new MemoryStream();
            memoryStream.Write(new byte[] { 1, 2, 3, }, 0, 3);
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
                using var target = new StreamByteSequence(fileName);
                target.GetNext().Should().Be((byte)'t');
                target.GetNext().Should().Be((byte)'e');
                target.GetNext().Should().Be((byte)'s');
                target.GetNext().Should().Be((byte)'t');
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
    }
}
