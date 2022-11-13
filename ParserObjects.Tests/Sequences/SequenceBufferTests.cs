using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class SequenceBufferTests
    {
        [Test]
        public void Indexer_Test()
        {
            var sequence = FromString("abcdefg");
            var buffer = sequence.CreateBuffer();
            buffer[0].Should().Be('a');
            buffer[1].Should().Be('b');
            buffer[2].Should().Be('c');
            buffer[3].Should().Be('d');
            buffer[4].Should().Be('e');

            var result = buffer.Capture(3);
            var str = new string(result);
            str.Should().Be("abc");

            sequence.Peek().Should().Be('d');
        }
    }
}
