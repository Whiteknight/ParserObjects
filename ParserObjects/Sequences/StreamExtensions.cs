using System.IO;
using System.Text;

namespace ParserObjects.Sequences
{
    public static class StreamExtensions
    {
        public static ISequence<byte> ToByteSequence(this Stream stream, string fileName = null)
            => new StreamByteSequence(stream, fileName);

        public static ISequence<char> ToCharSequence(this Stream stream, Encoding encoding = null, string fileName = null)
            => new StreamCharacterSequence(stream, encoding, fileName);

        public static ISequence<char> ToCharSequence(this StreamReader streamReader, string fileName = null)
            => new StreamCharacterSequence(streamReader, fileName);
    }
}
