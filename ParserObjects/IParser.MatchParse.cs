using System;
using ParserObjects.Internal;
using static ParserObjects.Sequences;

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
    public static bool Match<TInput>(this IParser<TInput> parser, ISequence<TInput> input)
        => parser.Match(new ParseState<TInput>(input, Defaults.LogMethod));

    /// <summary>
    /// Attempts a parse but does not consume any input. Instead it returns a boolean true if
    /// the parser would succeed or false otherwise.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool Match<TInput>(this IMultiParser<TInput> parser, ISequence<TInput> input)
    {
        var startCp = input.Checkpoint();
        var result = parser.Parse(new ParseState<TInput>(input, Defaults.LogMethod));
        startCp.Rewind();
        return result.Success;
    }

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Attempts a parse but does not
    /// consume any input. Returns true if the parse would succeed, false otherwise.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static bool Match(this IParser<char> parser, string input, SequenceOptions<char> options = default)
        => parser.Match(
            new ParseState<char>(
                FromString(input, options),
                Defaults.LogMethod
            )
        );

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Attempts a parse but
    /// does not consume any input. Returns true if the parse would succeed, false otherwise.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static bool Match(this IMultiParser<char> parser, string input, SequenceOptions<char> options = default)
        => parser.Parse(
            new ParseState<char>(
                FromString(input, options),
                Defaults.LogMethod
            )
        ).Success;

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Parse the given input string
    /// and return the first value or failure. Creates a character sequence from the string
    /// and the ParseState object.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="s"></param>
    /// <param name="options"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static Result<TOutput> Parse<TOutput>(
        this IParser<char, TOutput> parser,
        string s,
        SequenceOptions<char> options = default,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<char>(
            FromString(s, options),
            log ?? Defaults.LogMethod
        )
    );

    /// <summary>
    /// Convenience method for parsers which act on character sequences. Parse the given input
    /// string and return the first value or failure. Creates a character sequence from the
    /// string and the ParseState object.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="s"></param>
    /// <param name="options"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static Result<object> Parse(
        this IParser<char> parser,
        string s,
        SequenceOptions<char> options = default,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<char>(
            FromString(s, options),
            log ?? Defaults.LogMethod
        )
    );

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
    public static Result<TOutput> Parse<TInput, TOutput>(
        this IParser<TInput, TOutput> parser,
        ISequence<TInput> input,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<TInput>(input, log ?? Defaults.LogMethod)
    );

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
    public static MultiResult<TOutput> Parse<TInput, TOutput>(
        this IMultiParser<TInput, TOutput> parser,
        ISequence<TInput> input,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<TInput>(input, log ?? Defaults.LogMethod)
    );

    /// <summary>
    /// Covenience method to invoke a parser which does not return a value. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static Result<object> Parse<TInput>(
        this IParser<TInput> parser,
        ISequence<TInput> input,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<TInput>(input, log ?? Defaults.LogMethod)
    );

    /// <summary>
    /// Convenience method to invoke a parser which does not return a value. Creates the
    /// necessary ParseState.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static MultiResult<object> Parse<TInput>(
        this IMultiParser<TInput> parser,
        ISequence<TInput> input,
        Action<string>? log = null
    ) => parser.Parse(
        new ParseState<TInput>(input, log ?? Defaults.LogMethod)
    );
}
