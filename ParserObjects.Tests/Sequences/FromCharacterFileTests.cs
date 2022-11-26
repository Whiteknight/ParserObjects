using System.IO;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromCharacterFileTests
{
    [Test]
    public void FileStream_Test()
    {
        var fileName = Guid.NewGuid().ToString() + ".txt";
        ISequence<char> target = null;
        try
        {
            File.WriteAllText(fileName, "test");
            target = FromCharacterFile(fileName);
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('s');
            target.GetNext().Should().Be('t');
        }
        finally
        {
            (target as IDisposable)?.Dispose();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
