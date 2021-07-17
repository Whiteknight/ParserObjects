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
        /// <param name="bufferSize"></param>
        /// <param name="endSentinel"></param>
        /// <returns></returns>
        public static ISequence<byte> ToByteSequence(this Stream stream, string fileName = "", int bufferSize = 1024, byte endSentinel = 0)
            => new StreamByteSequence(stream, fileName, bufferSize, endSentinel);

        /// <summary>
        /// Converts an existing Stream to a sequence of char using the default UTF-8 encoding.
        /// Calling .Dispose() on the sequence will dispose the stream as well.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="bufferSize"></param>
        /// <param name="normalizeLineEndings"></param>
        /// <param name="endSentinel"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharSequence(this Stream stream, string fileName = "", int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
            => new StreamCharacterSequence(stream, Encoding.UTF8, fileName, bufferSize: bufferSize, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);

        /// <summary>
        /// Converts an existing Stream to a sequence of char using the provided encoding. Calling
        /// .Dispose() on the sequence will dispose the stream as well.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="fileName"></param>
        /// <param name="bufferSize"></param>
        /// <param name="normalizeLineEndings"></param>
        /// <param name="endSentinel"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharSequence(this Stream stream, Encoding encoding, string fileName = "", int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
            => new StreamCharacterSequence(stream, encoding, fileName, bufferSize: bufferSize, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);

        /// <summary>
        /// Converts an existing StreamReader to a sequence of char. Calling .Dispose() on the
        /// sequence will dispose the reader as well.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="fileName"></param>
        /// <param name="bufferSize"></param>
        /// <param name="normalizeLineEndings"></param>
        /// <param name="endSentinel"></param>
        /// <returns></returns>
        public static ISequence<char> ToCharSequence(this StreamReader streamReader, string fileName = "", int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
            => new StreamCharacterSequence(streamReader, fileName, bufferSize: bufferSize, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);
    }
}
