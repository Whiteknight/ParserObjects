using System.IO;
using System.Text;

namespace ParserObjects.Tests.Sequences;

public static class ToCharacterSequenceTests
{
    public class Streams
    {
        [Test]
        public void ToCharSequence_Stream()
        {
            var memoryStream = new MemoryStream();
            var b = Encoding.UTF8.GetBytes("abc");
            memoryStream.Write(b, 0, b.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var target = memoryStream.ToCharacterSequence();
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
            var target = reader.ToCharacterSequence();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }
    }

    public class Characters
    {
        [Test]
        public void AsCharacterSequence_GetNext_Test()
        {
            var target = "abc".ToCharacterSequence();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('\0');
        }
    }
}
