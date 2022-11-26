using System.IO;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromByteFileTests
{
    [Test]
    public void FileStream_Test()
    {
        var fileName = Guid.NewGuid().ToString() + ".txt";
        ISequence<byte> target = null;
        try
        {
            File.WriteAllText(fileName, "test");
            target = FromByteFile(fileName);
            target.GetNext().Should().Be((byte)'t');
            target.GetNext().Should().Be((byte)'e');
            target.GetNext().Should().Be((byte)'s');
            target.GetNext().Should().Be((byte)'t');
        }
        finally
        {
            (target as IDisposable)?.Dispose();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
