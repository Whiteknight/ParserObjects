using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static ISequence<byte> FromByteStream(
        Stream stream,
        SequenceOptions<byte> options = default
    ) => new StreamByteSequence(stream, options);

    /// <summary>
    /// Converts an existing  Stream to an ISequence of byte. Calling .Dispose() on the sequence
    /// will dispose the stream as well.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<byte> ToByteSequence(
        this Stream stream,
        SequenceOptions<byte> options = default
    ) => FromByteStream(stream, options);

    /// <summary>
    /// Return a char sequence from the file with the given name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static ICharSequence FromCharacterFile(string fileName, Encoding? encoding = null)
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
    public static ICharSequence FromCharacterFile(SequenceOptions<char> options)
    {
        options.Validate();
        var stream = File.OpenRead(options.FileName);
        if (stream.Length <= options.BufferSize)
        {
            using var tempReader = new StreamReader(stream, options.Encoding!);
            var s = tempReader.ReadToEnd();
            return FromString(s, options);
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
    public static ICharSequence FromCharacterStream(
        Stream stream,
        SequenceOptions<char> options = default
    )
    {
        options.Validate();
        if (stream.Length <= options.BufferSize)
        {
            using var reader = new StreamReader(stream);
            var s = reader.ReadToEnd();
            return FromString(s, options);
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
    public static ICharSequence FromCharacterStream(
        StreamReader reader,
        SequenceOptions<char> options = default
    ) => new StreamCharacterSequence(reader, options);

    /// <summary>
    /// Read the enumerable to an IReadOnlyList and wrap the list in an ISequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<T?> FromEnumerable<T>(
        IEnumerable<T> source,
        SequenceOptions<T?> options = default
    ) => new ListSequence<T?>(source, options.EndSentinel);

    /// <summary>
    /// Read the enumerable into an IReadOnlyList and wrap the list in an ICharSequence.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence FromEnumerable(
        IEnumerable<char> source,
        SequenceOptions<char> options = default
    ) => new CharBufferSequence.FromCharArray(source.ToList(), options);

    /// <summary>
    /// Wrap the list in an ISequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static ISequence<T?> FromList<T>(IReadOnlyList<T> list, T? endSentinel = default)
        => new ListSequence<T?>(list, endSentinel);

    /// <summary>
    /// Wrap the list of characters in an ICharSequence.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence FromList(
        IReadOnlyList<char> list,
        SequenceOptions<char> options = default
    ) => new CharBufferSequence.FromCharArray(list, options);

    /// <summary>
    /// Read the enumerable to a list and then wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="endSentinel">An end value to return when the sequence is exhausted.</param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IEnumerable<T> enumerable, T? endSentinel = default)
        => FromList(enumerable.ToList(), endSentinel);

    /// <summary>
    /// Wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="endValue"></param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IReadOnlyList<T> list, T? endValue = default)
        => FromList(list, endValue);

    /// <summary>
    /// Convert a function delegate to an ICharSequence. The function should take an index i, starting
    /// from 0, and return the input item at that index and a flag whether the input sequence has
    /// reached the end of input immediately after that item. Indices may be read in any order
    /// and may be read more than once.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence FromMethod(
        Func<int, (char Next, bool IsAtEnd)> function,
        SequenceOptions<char> options = default
    ) => new UserDelegate.CharSequence(function, options);

    /// <summary>
    /// Convert a function delegate to an ISequence. The function should take an index i, starting
    /// from 0, and return the input item at that index and a flag whether the input sequence has
    /// reached the end of input immediately after that item. Indices may be read in any order
    /// and may be read more than once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="function"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<T> FromMethod<T>(
        Func<int, (T Next, bool IsAtEnd)> function,
        SequenceOptions<T> options = default
    ) => new UserDelegate.Sequence<T>(function, options);

    /// <summary>
    /// Creates a sequence from the results of repeated invocation of the given parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="parser"></param>
    /// <param name="getEndSentinel"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static ISequence<Result<TResult>> FromParseResult<TInput, TResult>(
        ISequence<TInput> sequence,
        IParser<TInput, TResult> parser,
        Func<ResultFactory<TInput, TResult>, Result<TResult>>? getEndSentinel = null,
        Action<string>? log = null
    ) => new ParseResultSequence<TInput, TResult>(sequence, parser, getEndSentinel, log ?? (_ => { }));

    /// <summary>
    /// Return a sequence from the given string.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence FromString(string s, SequenceOptions<char> options = default)
        => options.NormalizeLineEndings
        ? new CharBufferSequence.FromCharArray(s, options)
        : new CharBufferSequence.FromNonnormalizedString(s, options);

    /// <summary>
    /// Wrap the string as a sequence of characters.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence ToCharacterSequence(
        this string str,
        SequenceOptions<char> options = default
    ) => FromString(str, options);

    /// <summary>
    /// Converts an existing Stream to a sequence of char using the default UTF-8 encoding.
    /// Calling .Dispose() on the sequence will dispose the stream as well.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence ToCharacterSequence(
        this Stream stream,
        SequenceOptions<char> options = default
    ) => FromCharacterStream(stream, options);

    /// <summary>
    /// Converts an existing StreamReader to a sequence of char. Calling .Dispose() on the
    /// sequence will dispose the reader as well.
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence ToCharacterSequence(
        this StreamReader streamReader,
        SequenceOptions<char> options = default
    ) => FromCharacterStream(streamReader, options);
}
