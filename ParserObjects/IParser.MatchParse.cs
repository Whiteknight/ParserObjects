using System;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects;

/// <summary>
/// General-purpose extensions for IParser and descendents.
/// </summary>
public static class ParserMatchParseExtensions
{
    /// <summary>
    /// Attempts a parse but does not consume any input. Instead it returns a boolean true if the parse
    /// succeeded or false otherwise.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CanMatch<TInput>(this IParser<TInput> parser, ISequence<TInput> input)
    {
        var checkpoint = input.Checkpoint();
        var state = new ParseState<TInput>(input, Defaults.LogMethod);
        var result = parser.Parse(state);
        checkpoint.Rewind();
        return result.Success;
    }

    /// <summary>
    /// Attempts a parse but does not consume any input. Instead it returns a boolean true if
    /// the parser would succeed or false otherwise.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CanMatch<TInput>(this IMultiParser<TInput> parser, ISequence<TInput> input)
    {
        var state = new ParseState<TInput>(input, Defaults.LogMethod);
        var result = parser.Parse(state);
        result.StartCheckpoint.Rewind();
        return result.Success;
    }

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Attempts a parse but does not
    /// consume any input. Returns true if the parse would succeed, false otherwise.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="normalizeLineEndings"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static bool CanMatch(this IParser<char> parser, string input, bool normalizeLineEndings = true, char endSentinel = '\0')
    {
        // Don't need to .Checkpoint()/.Rewind() because the sequence is private and we don't
        // reuse it
        var sequence = new StringCharacterSequence(input, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);
        var state = new ParseState<char>(sequence, Defaults.LogMethod);
        var result = parser.Parse(state);
        return result.Success;
    }

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Attempts a parse but
    /// does not consume any input. Returns true if the parse would succeed, false otherwise.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="normalizeLineEndings"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static bool CanMatch(this IMultiParser<char> parser, string input, bool normalizeLineEndings = true, char endSentinel = '\0')
    {
        // Don't need to .Checkpoint()/.Rewind() because the sequence is private and we don't
        // reuse it
        var sequence = new StringCharacterSequence(input, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);
        var state = new ParseState<char>(sequence, Defaults.LogMethod);
        var result = parser.Parse(state);
        return result.Success;
    }

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Parse the given input string
    /// and return the first value or failure. Creates a character sequence from the string
    /// and the ParseState object.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="s"></param>
    /// <param name="normalizeLineEndings"></param>
    /// <param name="endSentinel"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IResult<TOutput> Parse<TOutput>(this IParser<char, TOutput> parser, string s, bool normalizeLineEndings = true, char endSentinel = '\0', Action<string>? log = null)
        => parser.Parse(new ParseState<char>(new StringCharacterSequence(s, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel), log ?? Defaults.LogMethod));

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Parse the given input
    /// string and return the first value or failure. Creates a character sequence from the
    /// string and the ParseState object.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="s"></param>
    /// <param name="normalizeLineEndings"></param>
    /// <param name="endSentinel"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IResult Parse(this IParser<char> parser, string s, bool normalizeLineEndings = true, char endSentinel = '\0', Action<string>? log = null)
        => parser.Parse(new ParseState<char>(new StringCharacterSequence(s, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel), log ?? Defaults.LogMethod));

    /// <summary>
    /// Convenience method to invoke a parser which acts on an input sequence. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IResult<TOutput> Parse<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input, Action<string>? log = null)
        => parser.Parse(new ParseState<TInput>(input, log ?? Defaults.LogMethod));

    /// <summary>
    /// Convenience method to invoke a parser which acts on an input sequence. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IMultiResult<TOutput> Parse<TInput, TOutput>(this IMultiParser<TInput, TOutput> parser, ISequence<TInput> input, Action<string>? log = null)
        => parser.Parse(new ParseState<TInput>(input, log ?? Defaults.LogMethod));

    /// <summary>
    /// Covenience method to invoke a parser which does not return a value. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IResult Parse<TInput>(this IParser<TInput> parser, ISequence<TInput> input, Action<string>? log = null)
        => parser.Parse(new ParseState<TInput>(input, log ?? Defaults.LogMethod));

    /// <summary>
    /// Convenience method to invoke a parser which does not return a value. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IMultiResult Parse<TInput>(this IMultiParser<TInput> parser, ISequence<TInput> input, Action<string>? log = null)
        => parser.Parse(new ParseState<TInput>(input, log ?? Defaults.LogMethod));
}
