using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParserObjects.Internal.Sequences;

namespace ParserObjects;

public static class Sequences
{
    public static ICharSequenceWithRemainder FromString(string s, SequenceOptions<char> options = default)
        => new StringCharacterSequence(s, options);

    public static ISequence<char> FromCharacterFile(string fileName, Encoding? encoding = null)
        => FromCharacterFile(new SequenceOptions<char>
        {
            FileName = fileName,
            Encoding = encoding
        });

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

    public static ISequence<byte> FromByteFile(string fileName)
        => new StreamByteSequence(new SequenceOptions<byte>
        {
            FileName = fileName
        });

    public static ISequence<byte> FromByteFile(SequenceOptions<byte> options)
        => new StreamByteSequence(options);

    public static ISequence<byte> FromByteStream(Stream stream, SequenceOptions<byte> options = default)
        => new StreamByteSequence(stream, options);

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

    public static ISequence<char> FromCharacterStream(StreamReader reader, SequenceOptions<char> options = default)
    {
        options.Validate();
        if (reader.CurrentEncoding.IsSingleByte)
            return new StreamSingleByteCharacterSequence(reader, options);

        return new StreamCharacterSequence(reader, options);
    }

    public static ISequence<T?> FromEnumerable<T>(IEnumerable<T> source, T? endSentinel = default)
        => new ListSequence<T>(source, endSentinel);

    public static ISequence<T?> FromList<T>(IReadOnlyList<T> list, T? endSentinel = default)
        => new ListSequence<T>(list, endSentinel);

    public static ISequence<IResult<TResult>> FromParseResult<TInput, TResult>(ISequence<TInput> sequence, IParser<TInput, TResult> parser, Action<string>? log = null)
        => new ParseResultSequence<TInput, TResult>(sequence, parser, log ?? (_ => { }));
}
