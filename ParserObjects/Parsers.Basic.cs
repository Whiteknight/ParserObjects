﻿using System;
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
    {
        if (parsers == null || parsers.Length == 0)
            return Produce(static () => Array.Empty<TInput>());
        return new CaptureParser<TInput, TInput[]>(parsers, static (s, start, end) => s.GetBetween(start, end));
    }

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        GetParserFromResult<TInput, TMiddle, TOutput> getNext,
        params IParser[] mentions
    ) => new Chain<TInput, TOutput>.Parser<TMiddle, GetParserFromResult<TInput, TMiddle, TOutput>>(p, getNext, static (gn, r) => gn(r), mentions);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TOutput>(
        IParser<TInput> p,
        GetParserFromResult<TInput, TOutput> getNext,
        params IParser[] mentions
    ) => new Chain<TInput, TOutput>.Parser<GetParserFromResult<TInput, TOutput>>(p, getNext, static (gn, r) => gn(r), mentions);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        Action<ParserPredicateSelector<TInput, TMiddle, TOutput>> setup
    ) => Internal.Parsers.Chain<TInput, TOutput>.Configure<TMiddle>(p, setup);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TOutput>(
        IParser<TInput> p,
        Action<ParserPredicateSelector<TInput, TOutput>> setup
    ) => Internal.Parsers.Chain<TInput, TOutput>.Configure(p, setup);

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
    public static IParser<TInput, TOutput> Choose<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        Func<IResult<TMiddle>, IParser<TInput, TOutput>> getNext,
        params IParser[] mentions
    ) => new Chain<TInput, TOutput>.Parser<TMiddle, Func<IResult<TMiddle>, IParser<TInput, TOutput>>>(None(p), getNext, static (gn, r) => gn(r), mentions);

    /// <summary>
    /// Given a list of parsers, parse each in sequence and return a list of object
    /// results on success.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine(params IParser<TInput>[] parsers)
    {
        if (parsers == null || parsers.Length == 0)
            return Produce(static () => Array.Empty<object>());
        return Internal.Parsers.Rule.Create(
            parsers,
            Defaults.ObjectInstance,
            static (_, r) => r,
            true
        );
    }

    /// <summary>
    /// Given a list of parsers, parse each in sequence and return a list of object results on
    /// success.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine(IReadOnlyList<IParser<TInput>> parsers)
    {
        if (parsers == null || parsers.Count == 0)
            return Produce(static () => Array.Empty<object>());
        return Internal.Parsers.Rule.Create(
            parsers,
            Defaults.ObjectInstance,
            static (_, r) => r,
            true
        );
    }

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
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Examine<TOutput>(
        IParser<TInput, TOutput> parser,
        Action<ParseContext<TInput, TOutput>>? before = null,
        Action<ParseContext<TInput, TOutput>>? after = null
    )
    {
        if (before == null && after == null)
            return parser;
        return new Examine<TInput, TOutput>.Parser(parser, before, after);
    }

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Examine<TOutput>(
        IMultiParser<TInput, TOutput> parser,
        Action<MultiParseContext<TInput, TOutput>>? before = null,
        Action<MultiParseContext<TInput, TOutput>>? after = null
    )
    {
        if (before == null && after == null)
            return parser;
        return new Examine<TInput, TOutput>.MultiParser(parser, before, after);
    }

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput> Examine(
        IParser<TInput> parser,
        Action<ParseContext<TInput>>? before = null,
        Action<ParseContext<TInput>>? after = null
    )
    {
        if (before == null && after == null)
            return parser;
        return new ExamineParser<TInput>(parser, before, after);
    }

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
    {
        if (parsers == null || parsers.Length == 0)
            return Fail<TOutput>("No parsers given");
        if (parsers.Length == 1)
            return parsers[0];
        return new FirstParser<TInput>.WithOutput<TOutput>(parsers);
    }

    public static IParser<TInput> First(params IParser<TInput>[] parsers)
    {
        if (parsers == null || parsers.Length == 0)
            return Fail("No parsers given");
        if (parsers.Length == 1)
            return parsers[0];
        return new FirstParser<TInput>.WithoutOutput(parsers);
    }

    private readonly record struct FunctionArgs<TOutput>(
        Func<IParseState<TInput>, ResultFactory<TInput, TOutput>, IResult<TOutput>> ParseFunction,
        Func<IParseState<TInput>, bool>? MatchFunction
    );

    /// <summary>
    /// Invoke a function callback to perform the parse at the current location in the input
    /// stream.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parseFunction"></param>
    /// <param name="matchFunction"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Function<TOutput>(
        Func<IParseState<TInput>, ResultFactory<TInput, TOutput>, IResult<TOutput>> parseFunction,
        Func<IParseState<TInput>, bool>? matchFunction = null,
        string description = ""
    ) => new Function<TInput, TOutput>.Parser<FunctionArgs<TOutput>>(
        new FunctionArgs<TOutput>(parseFunction, matchFunction),
        static (state, f, args) => f.ParseFunction(state, args),
        matchFunction == null ? null : static (state, f) => f.MatchFunction!.Invoke(state),
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

                if (!result.Success)
                    return result;

                startCheckpoint.Rewind();
                return state.Success(p, result.Value, 0);
            },
            static (p, state) =>
            {
                var startCheckpoint = state.Input.Checkpoint();
                var result = p.Parse(state);

                if (!result.Success)
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
    /// Given the next input lookahead value, select the appropriate parser to use to continue
    /// the parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Predict<TOutput>(Action<ParserPredicateSelector<TInput, TInput, TOutput>> setup)
         => Internal.Parsers.Chain<TInput, TOutput>.Configure<TInput>(Peek(), setup);

    /// <summary>
    /// Produce a value without consuming anything out of the input sequence.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Produce<TOutput>(Func<TOutput> produce)
        => new Function<TInput, TOutput>.Parser<Func<TOutput>>(
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
        => new Function<TInput, TOutput>.Parser<Func<IParseState<TInput>, TOutput>>(
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
    public static IParser<TInput, TOutput> Sequential<TOutput>(Func<SequentialState<TInput>, TOutput> func)
        => new Sequential.Parser<TInput, TOutput, Func<SequentialState<TInput>, TOutput>>(func, static (s, d) => d(s));

    /// <summary>
    /// Execute a specially-structured callback to turn a parse into sequential, procedural code.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Sequential<TOutput, TData>(TData data, Func<SequentialState<TInput>, TData, TOutput> func)
        => new Sequential.Parser<TInput, TOutput, TData>(data, func);

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
    ) => new Transform<TInput, TMiddle, TOutput, Func<TMiddle, TOutput>>.Parser(parser, transform, static (t, v) => t(v));

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
    ) => new Transform<TInput, TMiddle, TOutput, TData>.Parser(parser, data, transform);

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
    ) => new Transform<TInput, TMiddle, TOutput, Func<TMiddle, TOutput>>.MultiParser(parser, transform, static (t, v) => t(v));

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
    public static IParser<TInput> Try(
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
