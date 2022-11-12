using System.Collections.Generic;
using System.IO;
using System.Text;
using ParserObjects.Sequences;

namespace ParserObjects;

public static class SequenceMethods
{
    public static ICharSequenceWithRemainder FromString(string s)
        => new StringCharacterSequence(s);

    public static ICharSequenceWithRemainder FromString(string s, StringCharacterSequence.Options options)
        => new StringCharacterSequence(s, options);

    public static ISequence<char> FromCharacterFile(string fileName, Encoding? encoding = null)
        => new StreamCharacterSequence(new StreamCharacterSequence.Options
        {
            FileName = fileName
        }, encoding);

    public static ISequence<byte> FromByteFile(string fileName)
        => new StreamByteSequence(new StreamByteSequence.Options(fileName, 0, 0));

    public static ISequence<byte> FromStream(Stream stream)
        => new StreamByteSequence(stream, default);

    public static ISequence<byte> FromStream(Stream stream, StreamByteSequence.Options options)
        => new StreamByteSequence(stream, options);

    public static ISequence<char> FromStream(Stream stream, Encoding encoding)
        => new StreamCharacterSequence(stream, default, encoding);

    public static ISequence<char> FromStream(Stream stream, Encoding encoding, StreamCharacterSequence.Options options)
        => new StreamCharacterSequence(stream, options, encoding);

    public static ISequence<T> FromEnumerable<T>(IEnumerable<T> source, T endSentinel = default)
        => new ListSequence<T>(source, endSentinel);

    public static ISequence<T> FromList<T>(IReadOnlyList<T> list, T endSentinel = default)
        => new ListSequence<T>(list, endSentinel);

    public static ISequence<IResult<TResult>> FromParseResult<TInput, TResult>(ISequence<TInput> sequence, IParser<TInput, TResult> parser)
        => new ParseResultSequence<TInput, TResult>(sequence, parser, s => { });
}
