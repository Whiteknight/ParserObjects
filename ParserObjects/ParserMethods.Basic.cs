using System;
using System.Collections.Generic;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Parser methods for building combinators using declarative syntax.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Matches anywhere in the sequence except at the end, and consumes 1 token of input.
        /// </summary>
        /// <returns></returns>
        public static IParser<TInput, TInput> Any() => new AnyParser<TInput>();

        /// <summary>
        /// Parses a parser, returns true if the parser succeeds, false if it fails.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, bool> Bool(IParser<TInput> p)
            => If(p, Produce(() => true), Produce(() => false));

        /// <summary>
        /// Executes a parser, and uses the value to determine the next parser to execute.
        /// </summary>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getNext"></param>
        /// <param name="mentions"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Chain<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Chain<TInput, TMiddle, TOutput>.GetParser getNext, params IParser[] mentions)
            => new Chain<TInput, TMiddle, TOutput>.Parser(p, getNext, mentions);

        /// <summary>
        /// Executes a parser, and uses the value to determine the next parser to execute.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getNext"></param>
        /// <param name="mentions"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Chain<TOutput>(IParser<TInput> p, Chain<TInput, TOutput>.GetParser getNext, params IParser[] mentions)
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
            => new Chain<TInput, TMiddle, TOutput>.Parser(p, setup);

        /// <summary>
        /// Executes a parser, and uses the value to determine the next parser to execute.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> ChainWith<TOutput>(IParser<TInput> p, Action<Chain<TInput, TOutput>.IConfiguration> setup)
            => new Chain<TInput, TOutput>.Parser(p, setup);

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
        public static IParser<TInput, TOutput> Choose<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Chain<TInput, TMiddle, TOutput>.GetParser getNext, params IParser[] mentions)
            => new Chain<TInput, TMiddle, TOutput>.Parser(None(p), getNext, mentions);

        /// <summary>
        /// Given a list of parsers, parse each in sequence and return a list of object
        /// results on success.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<object>> Combine(params IParser<TInput>[] parsers)
            => new RuleParser<TInput, IReadOnlyList<object>>(parsers, r => r);

        /// <summary>
        /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Deferred<TOutput>(Func<IParser<TInput, TOutput>> getParser)
            => new Deferred<TInput, TOutput>.Parser(getParser);

        public static IMultiParser<TInput, TOutput> Deferred<TOutput>(Func<IMultiParser<TInput, TOutput>> getParser)
            => new Deferred<TInput, TOutput>.MultiParser(getParser);

        /// <summary>
        /// The empty parser, consumers no input and always returns success at any point.
        /// </summary>
        /// <returns></returns>
        public static IParser<TInput> Empty() => new EmptyParser<TInput>();

        /// <summary>
        /// Matches affirmatively at the end of the input, fails everywhere else. Returns no value.
        /// </summary>
        /// <returns></returns>
        public static IParser<TInput> End() => new EndParser<TInput>();

        /// <summary>
        /// Matches affirmatively at the end of the input. Fails everywhere else. Returns a boolean value.
        /// </summary>
        /// <returns></returns>
        public static IParser<TInput, bool> IsEnd() => If(End(), Produce(() => true));

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
            => new Fail<TInput, TOutput>.Parser(error);

        /// <summary>
        /// Unconditionally returns failure.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IParser<TInput, TInput> Fail(string error = "Fail")
            => new Fail<TInput, TInput>.Parser(error);

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
        /// <returns></returns>
        public static IParser<TInput, TOutput> Function<TOutput>(ParserFunction<TInput, TOutput> func)
            => new FuncParser<TInput, TOutput>(func);

        /// <summary>
        /// Wraps the parser to guarantee that it consumes no input.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> None<TOutput>(IParser<TInput, TOutput> inner)
            => new NoneParser<TInput, TOutput>(inner);

        /// <summary>
        /// Wraps the parser to guarantee that it consumes no input.
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IParser<TInput> None(IParser<TInput> inner)
            => new NoneParser<TInput>(inner);

        /// <summary>
        /// Attempt to parse an item and return an object which holds a value on success.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, IOption<TOutput>> Optional<TOutput>(IParser<TInput, TOutput> p)
            => First(
                p.Transform(x => new SuccessOption<TOutput>(x)),
                Produce((_, _) => FailureOption<TOutput>.Instance)
            );

        /// <summary>
        /// Attempt to parse a parser and return a default value if the parser fails.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault)
        {
            Assert.ArgumentNotNull(getDefault, nameof(getDefault));
            return First(
                p,
                Produce((_, _) => getDefault())
            );
        }

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Produce<TInput, TOutput>.Function getDefault)
        {
            Assert.ArgumentNotNull(getDefault, nameof(getDefault));
            return First(
                p,
                Produce(getDefault)
            );
        }

        /// <summary>
        /// Return the next item of input without consuming any input. Returns failure at end of
        /// input, success otherwise.
        /// </summary>
        /// <returns></returns>
        public static IParser<TInput, TInput> Peek() => new PeekParser<TInput>();

        /// <summary>
        /// Given the next input lookahead value, select the appropriate parser to use to continue
        /// the parse.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Predict<TOutput>(Action<Chain<TInput, TInput, TOutput>.IConfiguration> setup)
             => new Chain<TInput, TInput, TOutput>.Parser(Peek(), setup);

        /// <summary>
        /// Produce a value without consuming anything out of the input sequence.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TOutput>(Func<TOutput> produce)
            => new Produce<TInput, TOutput>.Parser((_, _) => produce());

        /// <summary>
        /// Produces a value given the input sequence and the current contextual data.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TOutput>(Produce<TInput, TOutput>.Function produce)
            => new Produce<TInput, TOutput>.Parser(produce);

        public static IMultiParser<TInput, TOutput> ProduceMulti<TOutput>(Func<IEnumerable<TOutput>> produce)
            => new Produce<TInput, TOutput>.MultiParser((_, _) => produce());

        public static IMultiParser<TInput, TOutput> ProduceMulti<TOutput>(Produce<TInput, TOutput>.MultiFunction produce)
            => new Produce<TInput, TOutput>.MultiParser(produce);

        /// <summary>
        /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="defaultParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TOutput>(IParser<TInput, TOutput> defaultParser)
            => new Replaceable<TInput, TOutput>.Parser(defaultParser ?? new Fail<TInput, TOutput>.Parser());

        public static IMultiParser<TInput, TOutput> Replaceable<TOutput>(IMultiParser<TInput, TOutput> defaultParser)
            => new Replaceable<TInput, TOutput>.MultiParser(defaultParser ?? new Fail<TInput, TOutput>.MultiParser());

        /// <summary>
        /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TOutput>()
            => new Replaceable<TInput, TOutput>.Parser(new Fail<TInput, TOutput>.Parser());

        public static IMultiParser<TInput, TOutput> ReplaceableMulti<TOutput>()
            => new Replaceable<TInput, TOutput>.MultiParser(new Fail<TInput, TOutput>.MultiParser());

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
        /// Transform one node into another node to fit into the grammar.
        /// </summary>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
            => TransformResult(parser, (_, _, result) => result.Transform(transform));

        public static IMultiParser<TInput, TOutput> Transform<TMiddle, TOutput>(IMultiParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
            => TransformResult(parser, (_, _, result) => result.Transform(transform));

        /// <summary>
        /// Transform one result into another result. Allows modifying the result value and all
        /// result metadata.
        /// </summary>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> TransformResult<TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Transform<TInput, TMiddle, TOutput>.Function transform)
            => new Transform<TInput, TMiddle, TOutput>.Parser(parser, transform);

        public static IMultiParser<TInput, TOutput> TransformResult<TMiddle, TOutput>(IMultiParser<TInput, TMiddle> parser, Transform<TInput, TMiddle, TOutput>.MultiFunction transform)
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
            => new Try.Parser<TInput, TOutput>(parser, examine, bubble);

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
            => new Try.Parser<TInput>(parser, examine, bubble);

        /// <summary>
        /// Execute a parser and catch any unhandled exceptions which may be thrown by it. On
        /// receiving an exception, the input sequence is rewound to the location where Try started
        /// and options are provided to determine what actions to take.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="examine"></param>
        /// <param name="bubble"></param>
        /// <returns></returns>
        public static IMultiParser<TInput> Try(IMultiParser<TInput> parser, Action<Exception>? examine = null, bool bubble = false)
           => new Try.MultiParser<TInput>(parser, examine, bubble);

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
           => new Try.MultiParser<TInput, TOutput>(parser, examine, bubble);
    }
}
