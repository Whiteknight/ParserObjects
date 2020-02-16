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
        public void GetRemainder_Test()
        {
            var target = new StringCharacterSequence("abc");
            target.GetNext().Should().Be('a');
            target.GetRemainder().Should().Be("bc");
        }
    }
}
