using System;
using System.Collections.Generic;
using ParserObjects.Internal;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

/// <summary>
/// Parser methods for building combinators using declarative syntax.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static partial class Parsers<TInput>
{
    /// <summary>
    /// Invokes a parser, returns Success(true) if the parser succeeds, Success(false) if it fails.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, bool> Bool(IParser<TInput> parser)
        => new Function<TInput, bool>.Parser<IParser<TInput>>(
            parser,
            static (state, p, args) =>
            {
                var result = p.Parse(state);
                return args.Success(result.Success);
            },
            static (state, p) => p.Match(state),
            "IF %0",
            new[] { parser }
        );

    /// <summary>
    /// Invokes the inner parsers using the Match method, in sequence. Returns an array of all
    /// input items from the input sequence which were consumed during the match.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput[]> Capture(params IParser<TInput>[] parsers)
        => parsers == null || parsers.Length == 0
            ? Produce(static () => Array.Empty<TInput>())
            : new CaptureParser<TInput, TInput[]>(parsers, static (s, start, end) => s.GetArrayBetween(start, end));

    /// <summary>
    /// Given a list of parsers, parse each in sequence and return a list of object
    /// results on success.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine(params IParser<TInput>[] parsers)
        => parsers == null || parsers.Length == 0
            ? Produce(static () => (IReadOnlyList<object>)Array.Empty<object>())
            : Internal.Parsers.Rule.Create(
                parsers,
                Defaults.ObjectInstance,
                static (_, r) => r
            );

    /// <summary>
    /// Given a list of parsers, parse each in sequence and return a list of object results on
    /// success.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine(IReadOnlyList<IParser<TInput>> parsers)
        => parsers == null || parsers.Count == 0
            ? Produce(static () => (IReadOnlyList<object>)Array.Empty<object>())
            : Internal.Parsers.Rule.Create(
                parsers,
                Defaults.ObjectInstance,
                static (_, r) => r
            );

    public static IParser<TInput, IReadOnlyList<TItem>> Combine<TItem>(IReadOnlyList<IParser<TInput, TItem>> parsers)
        => parsers == null || parsers.Count == 0
            ? Produce(static () => (IReadOnlyList<TItem>)Array.Empty<TItem>())
            : Internal.Parsers.Rule.CreateTyped(
                parsers,
                Defaults.ObjectInstance,
                static (_, r) => r
            );

    /// <summary>
    /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Deferred<TOutput>(Func<IParser<TInput, TOutput>> getParser)
        => new DeferredParser<TInput, TOutput, IParser<TInput, TOutput>>(getParser);

    /// <summary>
    /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Deferred<TOutput>(Func<IMultiParser<TInput, TOutput>> getParser)
        => new DeferredParser<TInput, TOutput, IMultiParser<TInput, TOutput>>(getParser);

    /// <summary>
    /// Executes all the parsers from the current location and returns a MultiResult with all
    /// results.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Each<TOutput>(params IParser<TInput, TOutput>[] parsers)
        => new EachParser<TInput, TOutput>(parsers, string.Empty);

    /// <summary>
    /// Unconditionally returns failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Fail<TOutput>(string error = "Fail")
        => new FailParser<TInput, TOutput>(error);

    /// <summary>
    /// Unconditionally returns failure.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput> Fail(string error = "Fail")
        => new FailParser<TInput, TInput>(error);

    /// <summary>
    /// Returns a multi result with unconditional failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> FailMulti<TOutput>(string error = "Fail")
        => new FailParser<TInput, TOutput>(error);

    /// <summary>
    /// Return the result of the first parser which succeeds.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TOutput>(params IParser<TInput, TOutput>[] parsers)
        => parsers switch
        {
            { Length: 1 } => parsers[0],
            { Length: > 1 } => new FirstParser<TInput>.WithOutput<TOutput>(parsers),
            _ => Fail<TOutput>("No parsers given")
        };

    /// <summary>
    /// Returns success when any parser succeeds, failure otherwise.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, object> First(params IParser<TInput>[] parsers)
        => parsers switch
        {
            { Length: 1 } => Object(parsers[0]),
            { Length: > 1 } => new FirstParser<TInput>.WithoutOutput(parsers),
            _ => Fail<object>("No parsers given")
        };

    /// <summary>
    /// Invoke a function callback to perform the parse at the current location in the input
    /// stream.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parse"></param>
    /// <param name="match"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Function<TOutput>(
        Func<IParseState<TInput>, ResultFactory<TInput, TOutput>, Result<TOutput>> parse,
        Func<IParseState<TInput>, bool>? match = null,
        string description = ""
    ) => Function<TInput, TOutput>.Create(
        (parse, match),
        static (state, f, args) => f.parse(state, args),
        match == null ? null : static (state, f) => f.match!.Invoke(state),
        description ?? "",
        Array.Empty<IParser>()
    );

    /// <summary>
    /// Invokes the parser but rewinds the input sequence to ensure no input items are consumed.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> None<TOutput>(IParser<TInput, TOutput> parser)
        => Function<TInput, TOutput>.Create(
            parser,
            static (state, p, args) =>
            {
                var cp = state.Input.Checkpoint();
                var result = p.Parse(state);
                if (result.Success)
                {
                    cp.Rewind();
                    return args.Success(result.Value);
                }

                return args.Failure(result.ErrorMessage);
            },
            static (state, p) =>
            {
                var cp = state.Input.Checkpoint();
                var result = p.Match(state);
                if (result)
                {
                    cp.Rewind();
                    return true;
                }

                return false;
            },
            "(?=%0)",
            new[] { parser }
        );

    /// <summary>
    /// Invokes the parser but rewinds the input sequence to ensure no input items are consumed.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, object> None(IParser<TInput> parser)
        => Function<TInput, object>.Create(
            parser,
            static (state, p, _) =>
            {
                var startCheckpoint = state.Input.Checkpoint();
                var result = p.Parse(state);

                if (!result.Success)
                    return result;

                startCheckpoint.Rewind();
                return Result.Ok(p, result.Value, 0);
            },
            static (state, p) =>
            {
                var startCheckpoint = state.Input.Checkpoint();
                var result = p.Parse(state);

                if (!result.Success)
                    return false;

                startCheckpoint.Rewind();
                return true;
            },
            "(?=%0)",
            new[] { parser }
        );

    /// <summary>
    /// Converts a parser which returns a value of unknown type to one which returns object.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Object(IParser<TInput> parser)
        => new Objects<TInput>.Parser(parser);

    /// <summary>
    /// Attempt to invoke a parser. Returns success with an Option to contain the value of the
    /// inner parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, Option<TOutput>> Optional<TOutput>(IParser<TInput, TOutput> p)
        => new Optional<TInput, TOutput>.NoDefaultParser(p);

    /// <summary>
    /// Attempt to invoke a parser. Returns the result on success, a default value on failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getDefault"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Optional<TOutput>(
        IParser<TInput, TOutput> p,
        Func<TOutput> getDefault
    )
    {
        Assert.ArgumentNotNull(getDefault);
        return new Optional<TInput, TOutput>.DefaultValueParser(p, _ => getDefault());
    }

    /// <summary>
    /// Attempt to invoke a parser. Returns the result on success, a default value on failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getDefault"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Optional<TOutput>(
        IParser<TInput, TOutput> p,
        Func<IParseState<TInput>, TOutput> getDefault
    )
    {
        Assert.ArgumentNotNull(getDefault);
        return new Optional<TInput, TOutput>.DefaultValueParser(p, getDefault);
    }

    /// <summary>
    /// Produce a value without consuming anything out of the input sequence.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Produce<TOutput>(Func<TOutput> produce)
        => Function<TInput, TOutput>.Create(
            produce,
            static (_, p, args) =>
            {
                var value = p();
                return args.Success(value);
            },
            static (_, _) => true,
            "PRODUCE",
            Array.Empty<IParser>()
        );

    /// <summary>
    /// Produce a value, possibly using data or input from the current parse state.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Produce<TOutput>(Func<IParseState<TInput>, TOutput> produce)
        => Function<TInput, TOutput>.Create(
            produce,
            static (state, p, args) =>
            {
                var value = p(state);
                return args.Success(value);
            },
            static (_, _) => true,
            "PRODUCE",
            Array.Empty<IParser>()
        );

    /// <summary>
    /// Produces a multi result with all returned values as alternatives.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ProduceMulti<TOutput>(Func<IEnumerable<TOutput>> produce)
        => Function<TInput, TOutput>.CreateMulti(produce, static (_, p, builder) =>
        {
            var values = p();
            return builder.AddSuccesses(values).BuildResult();
        }, "PRODUCE", Array.Empty<IParser>());

    /// <summary>
    /// Produces a multi result with all returned values as alternatives. Uses data or input from
    /// the current parse state.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ProduceMulti<TOutput>(Func<IParseState<TInput>, IEnumerable<TOutput>> produce)
        => Function<TInput, TOutput>.CreateMulti(produce, static (s, p, builder) =>
        {
            var values = p(s);
            return builder.AddSuccesses(values).BuildResult();
        }, "PRODUCE", Array.Empty<IParser>());

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TOutput>(IParser<TInput, TOutput> defaultParser)
        => Replaceable<TInput, TOutput>.From(
            defaultParser ?? Fail<TOutput>()
        );

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Replaceable(IParser<TInput> defaultParser)
        => Replaceable<TInput, object>.From(
            Objects<TInput>.AsObject(defaultParser ?? Fail<object>())
        );

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Replaceable<TOutput>(IMultiParser<TInput, TOutput> defaultParser)
        => Replaceable<TInput, TOutput>.From(
            defaultParser ?? FailMulti<TOutput>()
        );

    /// <summary>
    /// Serves as a placeholder in the parser graph where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TOutput>()
        => Replaceable<TInput, TOutput>.From(
            Fail<TOutput>()
        );

    /// <summary>
    /// Serves as a placeholder in the parser graph where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ReplaceableMulti<TOutput>()
        => Replaceable<TInput, TOutput>.From(
            FailMulti<TOutput>()
        );

    /// <summary>
    /// Attempt the parse. Return on success. On failure, enter "panic mode" where input tokens can be
    /// discarded until the next "good" location and the parse will be attempted again. Subsequent
    /// attempts will always return failure, but with error information about all the errors which
    /// were seen.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="attempt"></param>
    /// <param name="discardUntil"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Synchronize<TOutput>(
        IParser<TInput, TOutput> attempt,
        Func<TInput, bool> discardUntil
    ) => new SynchronizeParser<TInput, TOutput>(attempt, discardUntil);

    /// <summary>
    /// Transform the output value of the parser.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Transform<TMiddle, TOutput>(
        IParser<TInput, TMiddle> parser,
        Func<TMiddle, TOutput> transform
    ) => Transform<TInput>.Create(parser, transform, static (t, v) => t(v));

    /// <summary>
    /// Transform the output value of the parser.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="parser"></param>
    /// <param name="data"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Transform<TMiddle, TOutput, TData>(
        IParser<TInput, TMiddle> parser,
        TData data,
        Func<TData, TMiddle, TOutput> transform
    ) => Transform<TInput>.Create(parser, data, transform);

    /// <summary>
    /// Transforms the output value of the parser.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Transform<TMiddle, TOutput>(
        IMultiParser<TInput, TMiddle> parser,
        Func<TMiddle, TOutput> transform
    ) => Transform<TInput>.Create(parser, transform, static (t, v) => t(v));

    /// <summary>
    /// Execute a parser and catch any unhandled exceptions which may be thrown by it. On
    /// receiving an exception, the input sequence is rewound to the location where Try started
    /// and options are provided to determine what actions to take.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="examine"></param>
    /// <param name="bubble"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Try<TOutput>(
        IParser<TInput, TOutput> parser,
        Action<Exception>? examine = null,
        bool bubble = false
    ) => new TryParser<TInput>.Parser<TOutput>(parser, examine, bubble);

    /// <summary>
    /// Execute a parser and catch any unhandled exceptions which may be thrown by it. On
    /// receiving an exception, the input sequence is rewound to the location where Try started
    /// and options are provided to determine what actions to take.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="examine"></param>
    /// <param name="bubble"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Try(
        IParser<TInput> parser,
        Action<Exception>? examine = null,
        bool bubble = false
    ) => new TryParser<TInput>.Parser(parser, examine, bubble);

    /// <summary>
    /// Execute a parser and catch any unhandled exceptions which may be thrown by it. On
    /// receiving an exception, the input sequence is rewound to the location where Try started
    /// and options are provided to determine what actions to take.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="examine"></param>
    /// <param name="bubble"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Try<TOutput>(
        IMultiParser<TInput, TOutput> parser,
        Action<Exception>? examine = null,
        bool bubble = false
    ) => new TryParser<TInput>.MultiParser<TOutput>(parser, examine, bubble);
}
