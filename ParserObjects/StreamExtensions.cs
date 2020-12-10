using System.IO;
using System.Text;
using ParserObjects.Sequences;

namespace ParserObjects
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts an existing  Stream to an ISequence of byte. Calling .Dispose() on the sequence
        /// will dispose the stream as well.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ISequence<byte> ToByteSequence(this Stream stream, string fileName = null)
            => new StreamByteSequence(stream, fileName);

        /// <summary>
        /// Converts an existing Stream to a sequence of char using the provided encoding (UTF-8 by
        /// default). Calling .Dispose() on the sequence will dispose the stream as well.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharSequence(this Stream stream, Encoding encoding = null, string fileName = null)
            => new StreamCharacterSequence(stream, encoding, fileName);

        /// <summary>
        /// Converts an existing StreamReader to a sequence of char. Calling .Dispose() on the
        /// sequence will dispose the reader as well.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharSequence(this StreamReader streamReader, string fileName = null)
            => new StreamCharacterSequence(streamReader, fileName);
    }
}
