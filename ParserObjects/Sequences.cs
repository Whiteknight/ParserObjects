using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParserObjects.Internal.Sequences;

namespace ParserObjects;

public static class Sequences
{
    /// <summary>
    /// Return a byte sequence from the file with the given name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static ISequence<byte> FromByteFile(string fileName)
        => new StreamByteSequence(new SequenceOptions<byte>
        {
            FileName = fileName
        });

    /// <summary>
    /// Return a byte sequence from a file. The file name should be set in the options.FileName
    /// property.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<byte> FromByteFile(SequenceOptions<byte> options)
        => new StreamByteSequence(options);

    /// <summary>
    /// Return a byte sequence from the given stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<byte> FromByteStream(Stream stream, SequenceOptions<byte> options = default)
        => new StreamByteSequence(stream, options);

    /// <summary>
    /// Return a char sequence from the file with the given name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static ISequence<char> FromCharacterFile(string fileName, Encoding? encoding = null)
        => FromCharacterFile(new SequenceOptions<char>
        {
            FileName = fileName,
            Encoding = encoding
        });

    /// <summary>
    /// Return a char sequence from a file. The filename should be specified in the
    /// options.FileName property. Notice that different implementation types may be returned
    /// depending on the nature of the arguments provided.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> FromCharacterFile(SequenceOptions<char> options)
    {
        options.Validate();
        var stream = File.OpenRead(options.FileName);
        if (stream.Length <= options.BufferSize)
        {
            using var tempReader = new StreamReader(stream, options.Encoding!);
            var s = tempReader.ReadToEnd();
            return new StringCharacterSequence(s, options);
        }

        if (options.Encoding!.IsSingleByte)
            return new StreamSingleByteCharacterSequence(stream, options);

        return new StreamCharacterSequence(stream, options);
    }

    /// <summary>
    /// Return a char sequence from a stream. The stream is assumed to use options.Encoding as the
    /// encoding, using UTF8 by default. May return different implementations depending on the
    /// options passed.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> FromCharacterStream(Stream stream, SequenceOptions<char> options = default)
    {
        options.Validate();
        if (stream.Length <= options.BufferSize)
        {
            using var reader = new StreamReader(stream);
            var s = reader.ReadToEnd();
            return new StringCharacterSequence(s, options);
        }

        if (options.Encoding!.IsSingleByte)
            return new StreamSingleByteCharacterSequence(stream, options);

        return new StreamCharacterSequence(stream, options);
    }

    /// <summary>
    /// Returns a char sequence from the given StreamReader. options.Encoding will be ignored.
    /// May return different implementations depending on the options passed.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> FromCharacterStream(StreamReader reader, SequenceOptions<char> options = default)
        => new StreamCharacterSequence(reader, options);

    /// <summary>
    /// Read the enumerable to an IReadOnlyList and wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static ISequence<T?> FromEnumerable<T>(IEnumerable<T> source, T? endSentinel = default)
        => new ListSequence<T>(source, endSentinel);

    /// <summary>
    /// Wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static ISequence<T?> FromList<T>(IReadOnlyList<T> list, T? endSentinel = default)
        => new ListSequence<T>(list, endSentinel);

    /// <summary>
    /// Creates a sequence from the results of repeated invocation of the given parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="parser"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static ISequence<IResult<TResult>> FromParseResult<TInput, TResult>(ISequence<TInput> sequence, IParser<TInput, TResult> parser, Action<string>? log = null)
        => new ParseResultSequence<TInput, TResult>(sequence, parser, log ?? (_ => { }));

    /// <summary>
    /// Return a sequence from the given string.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequenceWithRemainder FromString(string s, SequenceOptions<char> options = default)
        => new StringCharacterSequence(s, options);
}
