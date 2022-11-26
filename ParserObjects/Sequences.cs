﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using ParserObjects.Internal.Sequences;

namespace ParserObjects;

public static class Sequences
{
    public static ICharSequenceWithRemainder FromString(string s, SequenceOptions<char> options = default)
        => new StringCharacterSequence(s, options);

    public static ISequence<char> FromCharacterFile(string fileName, Encoding? encoding = null)
        => new StreamCharacterSequence(new SequenceOptions<char>
        {
            FileName = fileName,
            Encoding = encoding
        });

    public static ISequence<char> FromCharacterFile(SequenceOptions<char> options)
        => new StreamCharacterSequence(options);

    public static ISequence<byte> FromByteFile(string fileName)
        => new StreamByteSequence(new SequenceOptions<byte>
        {
            FileName = fileName
        });

    public static ISequence<byte> FromByteFile(SequenceOptions<byte> options)
        => new StreamByteSequence(options);

    public static ISequence<byte> FromStream(Stream stream)
        => new StreamByteSequence(stream, default);

    public static ISequence<byte> FromStream(Stream stream, SequenceOptions<byte> options)
        => new StreamByteSequence(stream, options);

    public static ISequence<char> FromStream(Stream stream, SequenceOptions<char> options)
        => new StreamCharacterSequence(stream, options);

    public static ISequence<T> FromEnumerable<T>(IEnumerable<T> source, T endSentinel = default)
        => new ListSequence<T>(source, endSentinel);

    public static ISequence<T> FromList<T>(IReadOnlyList<T> list, T endSentinel = default)
        => new ListSequence<T>(list, endSentinel);

    public static ISequence<IResult<TResult>> FromParseResult<TInput, TResult>(ISequence<TInput> sequence, IParser<TInput, TResult> parser)
        => new ParseResultSequence<TInput, TResult>(sequence, parser, s => { });
}
