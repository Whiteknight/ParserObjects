using System.IO;

namespace ParserObjects.Tests.Sequences;

public class ToByteSequenceTests
{
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
}
