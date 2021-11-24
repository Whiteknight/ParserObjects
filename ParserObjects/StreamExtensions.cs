using System.IO;
using System.Text;
using ParserObjects.Sequences;

namespace ParserObjects;

public static class StreamExtensions
{
    /// <summary>
    /// Converts an existing  Stream to an ISequence of byte. Calling .Dispose() on the sequence
    /// will dispose the stream as well.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<byte> ToByteSequence(this Stream stream, StreamByteSequence.Options options = default)
        => new StreamByteSequence(stream, options);

    /// <summary>
    /// Converts an existing Stream to a sequence of char using the default UTF-8 encoding.
    /// Calling .Dispose() on the sequence will dispose the stream as well.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> ToCharSequence(this Stream stream, Encoding? encoding = null, StreamCharacterSequence.Options options = default)
        => new StreamCharacterSequence(stream, options, encoding ?? Encoding.UTF8);

    /// <summary>
    /// Converts an existing StreamReader to a sequence of char. Calling .Dispose() on the
    /// sequence will dispose the reader as well.
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> ToCharSequence(this StreamReader streamReader, StreamCharacterSequence.Options options = default)
        => new StreamCharacterSequence(streamReader, options);
}
