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
            var target = "abc".AsCharacterSequence();
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
        public void GetRemainder_OnePutback()
        {
            // We have a small optimization in for this case, so I'm adding a test for it
            var target = new StringCharacterSequence("abc");
            target.PutBack('Z');
            target.GetRemainder().Should().Be("Zabc");
        }

        [Test]
        public void GetRemainder_Putbacks()
        {
            var target = new StringCharacterSequence("abc");
            // Putbacks are first-in, last-out. So we should see WXYZ in the output
            target.PutBack('Z');
            target.PutBack('Y');
            target.PutBack('X');
            target.PutBack('W');
            target.GetRemainder().Should().Be("WXYZabc");
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
    }
}
