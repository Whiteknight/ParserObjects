using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

/// <summary>
/// Parser methods for building combinators using declarative syntax.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static partial class Parsers<TInput>
{
    private static readonly IParser<TInput, TInput> _any = new AnyParser<TInput>();
    private static readonly IParser<TInput> _empty = new EmptyParser<TInput>();
    private static readonly IParser<TInput> _end = new EndParser<TInput>();

    private static readonly IParser<TInput, bool> _isEnd = new Function<TInput, bool>.Parser<object>(
        Defaults.ObjectInstance,
        static (_, args) => args.Input.IsAtEnd ? args.Success(true) : args.Failure(""),
        static (_, args) => args.Input.IsAtEnd,
        "IF END THEN PRODUCE",
        Array.Empty<IParser>()
    );

    private static readonly IParser<TInput, TInput> _peek = new PeekParser<TInput>();

    /// <summary>
    /// Matches anywhere in the sequence except at the end, and consumes 1 token of input.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, TInput> Any() => _any;

    /// <summary>
    /// Invokes a parser, returns Success(true) if the parser succeeds, Success(false) if it fails.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, bool> Bool(IParser<TInput> parser)
        => new Function<TInput, bool>.Parser<IParser<TInput>>(
            parser,
            static (p, args) =>
            {
                var result = p.Parse(args.State);
                return args.Success(result.Success);
            },
            static (p, args) => p.Match(args.State),
            "IF {child}",
            new[] { parser }
        );

    /// <summary>
    /// Invokes the inner parsers using the Match method, in sequence. Returns an array of all
    /// input items from the input sequence which were consumed during the match.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput[]> Capture(params IParser<TInput>[] parsers)
        => new CaptureParser<TInput>(parsers);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Func<IResult<TMiddle>, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => new Chain<TInput, TMiddle, TOutput>.Parser(p, getNext, mentions);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TOutput>(IParser<TInput> p, Func<IResult, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => new Chain<TInput, TOutput>.Parser(p, getNext, mentions);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Action<Chain<TInput, TMiddle, TOutput>.IConfiguration> setup)
        => Chain<TInput, TMiddle, TOutput>.Configure(p, setup);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TOutput>(IParser<TInput> p, Action<Chain<TInput, TOutput>.IConfiguration> setup)
        => Internal.Parsers.Chain<TInput, TOutput>.Configure(p, setup);

    /// <summary>
    /// Executes a parser without consuming any input, and uses the value to determine the next
    /// parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Choose<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Func<IResult<TMiddle>, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => new Chain<TInput, TMiddle, TOutput>.Parser(None(p), getNext, mentions);

    /// <summary>
    /// Given a list of parsers, parse each in sequence and return a list of object
    /// results on success.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine(params IParser<TInput>[] parsers)
        => new RuleParser<TInput, IReadOnlyList<object>, object>(parsers, Defaults.ObjectInstance, static (_, r) => r);

    /// <summary>
    /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Deferred<TOutput>(Func<IParser<TInput, TOutput>> getParser)
        => new Deferred<TInput, TOutput>.Parser(getParser);

    /// <summary>
    /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Deferred<TOutput>(Func<IMultiParser<TInput, TOutput>> getParser)
        => new Deferred<TInput, TOutput>.MultiParser(getParser);

    /// <summary>
    /// Executes all the parsers from the current location and returns a multiresult with all
    /// results.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Each<TOutput>(params IParser<TInput, TOutput>[] parsers)
        => new EachParser<TInput, TOutput>(parsers, string.Empty);

    /// <summary>
    /// The empty parser, consumers no input and always returns success at any point.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput> Empty() => _empty;

    /// <summary>
    /// Matches affirmatively at the end of the input, fails everywhere else. Returns no value.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput> End() => _end;

    /// <summary>
    /// Matches affirmatively at the end of the input. Fails everywhere else. Returns a boolean value.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, bool> IsEnd() => _isEnd;

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Examine<TOutput>(IParser<TInput, TOutput> parser, Action<Examine<TInput, TOutput>.Context>? before = null, Action<Examine<TInput, TOutput>.Context>? after = null)
        => new Examine<TInput, TOutput>.Parser(parser, before, after);

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Examine<TOutput>(IMultiParser<TInput, TOutput> parser, Action<Examine<TInput, TOutput>.MultiContext>? before = null, Action<Examine<TInput, TOutput>.MultiContext>? after = null)
        => new Examine<TInput, TOutput>.MultiParser(parser, before, after);

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput> Examine(IParser<TInput> parser, Action<Examine<TInput>.Context>? before = null, Action<Examine<TInput>.Context>? after = null)
        => new Examine<TInput>.Parser(parser, before, after);

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
        => new FirstParser<TInput, TOutput>(parsers);

    /// <summary>
    /// Invoke a function callback to perform the parse at the current location in the input
    /// stream.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="func"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Function<TOutput>(Func<Function<TInput, TOutput>.SingleArguments, IResult<TOutput>> func, string description = "")
        => new Function<TInput, TOutput>.Parser<Func<Function<TInput, TOutput>.SingleArguments, IResult<TOutput>>>(
            func,
            static (f, args) => f(args),
            null,
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
        => new Function<TInput, TOutput>.Parser<IParser<TInput, TOutput>>(
            parser,
            static (p, args) =>
            {
                var cp = args.Input.Checkpoint();
                var result = p.Parse(args.State);
                if (result.Success)
                {
                    cp.Rewind();
                    return args.Success(result.Value, result.Location);
                }

                return args.Failure(result.ErrorMessage, result.Location);
            },
            static (p, args) =>
            {
                var cp = args.Input.Checkpoint();
                var result = p.Match(args.State);
                if (result)
                {
                    cp.Rewind();
                    return true;
                }

                return false;
            },
            "(?={child})",
            new[] { parser }
        );

    /// <summary>
    /// Invokes the parser but rewinds the input sequence to ensure no input items are consumed.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput> None(IParser<TInput> parser)
        => new Function<TInput>.Parser<IParser<TInput>>(
            parser,
            static (p, state) =>
            {
                var startCheckpoint = state.Input.Checkpoint();
                var result = p.Parse(state);

                if (!result.Success || result.Consumed == 0)
                    return result;

                startCheckpoint.Rewind();
                return state.Success(p, result.Value, 0, result.Location);
            },
            static (p, state) =>
            {
                var startCheckpoint = state.Input.Checkpoint();
                var result = p.Parse(state);

                if (!result.Success || result.Consumed == 0)
                    return false;

                startCheckpoint.Rewind();
                return true;
            },
            "(?={child})",
            new[] { parser }
        );

    /// <summary>
    /// AAttempt to invoke a parser. Returns an Option with the value on success.
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
    public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault)
    {
        Assert.ArgumentNotNull(getDefault, nameof(getDefault));
        return new Optional<TInput, TOutput>.DefaultValueParser(p, _ => getDefault());
    }

    /// <summary>
    /// Attempt to invoke a parser. Returns the result on success, a default value on failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getDefault"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Func<IParseState<TInput>, TOutput> getDefault)
    {
        Assert.ArgumentNotNull(getDefault, nameof(getDefault));
        return new Optional<TInput, TOutput>.DefaultValueParser(p, getDefault);
    }

    /// <summary>
    /// Return the next item of input without consuming any input. Returns failure at end of
    /// input, success otherwise.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, TInput> Peek() => _peek;

    /// <summary>
    /// Given the next input lookahead value, select the appropriate parser to use to continue
    /// the parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Predict<TOutput>(Action<Chain<TInput, TInput, TOutput>.IConfiguration> setup)
         => Chain<TInput, TInput, TOutput>.Configure(Peek(), setup);

    /// <summary>
    /// Produce a value without consuming anything out of the input sequence.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Produce<TOutput>(Func<TOutput> produce)
        => new Function<TInput, TOutput>.Parser<Func<TOutput>>(
            produce,
            static (p, args) =>
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
        => new Function<TInput, TOutput>.Parser<Func<IParseState<TInput>, TOutput>>(
            produce,
            static (p, args) =>
            {
                var value = p(args.State);
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
        => new Function<TInput, TOutput>.MultiParser<Func<IEnumerable<TOutput>>>(produce, static (p, builder) =>
        {
            var values = p();
            builder.AddSuccesses(values);
        }, "PRODUCE", Array.Empty<IParser>());

    /// <summary>
    /// Produces a multi result with all returned values as alternatives. Uses data or input from
    /// the current parse state.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ProduceMulti<TOutput>(Func<IParseState<TInput>, IEnumerable<TOutput>> produce)
        => new Function<TInput, TOutput>.MultiParser<Func<IParseState<TInput>, IEnumerable<TOutput>>>(produce, static (p, builder) =>
        {
            var values = p(builder.State);
            builder.AddSuccesses(values);
        }, "PRODUCE", Array.Empty<IParser>());

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TOutput>(IParser<TInput, TOutput> defaultParser)
        => new Replaceable<TInput, TOutput>.SingleParser(defaultParser ?? new FailParser<TInput, TOutput>());

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IParser<TInput> Replaceable(IParser<TInput> defaultParser)
        => new Replaceable<TInput>.SingleParser(defaultParser ?? new FailParser<TInput, object>());

    /// <summary>
    /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="defaultParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Replaceable<TOutput>(IMultiParser<TInput, TOutput> defaultParser)
        => new Replaceable<TInput, TOutput>.MultiParser(defaultParser ?? new FailParser<TInput, TOutput>());

    /// <summary>
    /// Serves as a placeholder in the parser graph where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TOutput>()
        => new Replaceable<TInput, TOutput>.SingleParser(new FailParser<TInput, TOutput>());

    /// <summary>
    /// Serves as a placeholder in the parser graph where an in-place replacement can be made.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ReplaceableMulti<TOutput>()
        => new Replaceable<TInput, TOutput>.MultiParser(new FailParser<TInput, TOutput>());

    /// <summary>
    /// Execute a specially-structured callback to turn a parse into sequential, procedural
    /// code.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Sequential<TOutput>(Func<Sequential.State<TInput>, TOutput> func)
        => new Sequential.Parser<TInput, TOutput>(func);

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
    public static IParser<TInput, TOutput> Synchronize<TOutput>(IParser<TInput, TOutput> attempt, Func<TInput, bool> discardUntil)
        => new SynchronizeParser<TInput, TOutput>(attempt, discardUntil);

    /// <summary>
    /// Transform the output value of the parser.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Transform<TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        => new Transform<TInput, TMiddle, TOutput>.Parser(parser, transform);

    /// <summary>
    /// Transforms the output value of the parser.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Transform<TMiddle, TOutput>(IMultiParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        => new Transform<TInput, TMiddle, TOutput>.MultiParser(parser, transform);

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
    public static IParser<TInput, TOutput> Try<TOutput>(IParser<TInput, TOutput> parser, Action<Exception>? examine = null, bool bubble = false)
        => new TryParser<TInput>.Parser<TOutput>(parser, examine, bubble);

    /// <summary>
    /// Execute a parser and catch any unhandled exceptions which may be thrown by it. On
    /// receiving an exception, the input sequence is rewound to the location where Try started
    /// and options are provided to determine what actions to take.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="examine"></param>
    /// <param name="bubble"></param>
    /// <returns></returns>
    public static IParser<TInput> Try(IParser<TInput> parser, Action<Exception>? examine = null, bool bubble = false)
        => new TryParser<TInput>.Parser(parser, examine, bubble);

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
    public static IMultiParser<TInput, TOutput> Try<TOutput>(IMultiParser<TInput, TOutput> parser, Action<Exception>? examine = null, bool bubble = false)
       => new TryParser<TInput>.MultiParser<TOutput>(parser, examine, bubble);
}
