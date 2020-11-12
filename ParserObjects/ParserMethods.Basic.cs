using System;
using System.Collections.Generic;
using ParserObjects.Parsers;

namespace ParserObjects
{
    /// <summary>
    /// Parser methods for building combinators using declarative syntax
    /// </summary>
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Matches anywhere in the sequence except at the end, and consumes 1 token of input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, TInput> Any() => new AnyParser<TInput>();

        public static IParser<TInput, TOutput> Chain<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Func<TMiddle, IParser<TInput, TOutput>> getNext)
            => new ChainParser<TInput, TMiddle, TOutput>(p, getNext);

        public static IParser<TInput, TOutput> Choose<TMiddle, TOutput>(IParser<TInput, TMiddle> p, Func<TMiddle, IParser<TInput, TOutput>> getNext)
            => new ChooseParser<TInput, TMiddle, TOutput>(p, getNext);

        /// <summary>
        /// Given a list of parsers, parse each in sequence and return a list of object
        /// results on success.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<object>> Combine(params IParser<TInput>[] parsers)
            => new RuleParser<TInput, IReadOnlyList<object>>(parsers, r => r);

        /// <summary>
        /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Deferred<TOutput>(Func<IParser<TInput, TOutput>> getParser)
            => new DeferredParser<TInput, TOutput>(getParser);

        /// <summary>
        /// The empty parser, consumers no input and always returns success at any point.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, object> Empty() => new EmptyParser<TInput>();

        /// <summary>
        /// Matches affirmatively at the end of the input, fails everywhere else.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, bool> End() => new EndParser<TInput>();

        /// <summary>
        /// Invoke callbacks before and after a parse
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Examine<TOutput>(IParser<TInput, TOutput> parser, Action<ExamineParseState<TInput, TOutput>> before = null, Action<ExamineParseState<TInput, TOutput>> after = null)
            => new ExamineParser<TInput, TOutput>(parser, before, after);

        /// <summary>
        /// A parser which unconditionally returns failure.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Fail<TOutput>() => new FailParser<TInput, TOutput>();

        /// <summary>
        /// Return the result of the first parser which succeeds
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> First<TOutput>(params IParser<TInput, TOutput>[] parsers)
            => new FirstParser<TInput, TOutput>(parsers);

        /// <summary>
        /// Flattens the result of a parser which returns an enumerable type into a parser which returns
        /// individual items.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Flatten<TCollection, TOutput>(IParser<TInput, TCollection> parser)
            where TCollection : IEnumerable<TOutput>
            => new FlattenParser<TInput, TCollection, TOutput>(parser);

        /// <summary>
        /// Invoke a function callback to perform the parse at the current location in the input
        /// stream
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Function<TOutput>(Func<ParseState<TInput>, IResult<TOutput>> func)
            => new FuncParser<TInput, TOutput>((t, success, fail) => func(t));

        public static IParser<TInput, TOutput> Function<TOutput>(ParserFunction<TInput, TOutput> func)
            => new FuncParser<TInput, TOutput>(func);

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
            => First(p, Produce(getDefault ?? (() => default)));

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TOutput>(IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> getDefault)
            => First(p, Produce(getDefault ?? (t => default)));

        /// <summary>
        /// Produce a value without consuming anything out of the input sequence
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TOutput>(Func<TOutput> produce)
            => new ProduceParser<TInput, TOutput>(t => produce());

        /// <summary>
        /// Produce a value given the current state of the input sequence.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TOutput>(Func<ISequence<TInput>, TOutput> produce)
            => new ProduceParser<TInput, TOutput>(produce);

        /// <summary>
        /// Serves as a placeholder in the parser tree where an in-place replacement can be made.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="defaultParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TOutput>(IParser<TInput, TOutput> defaultParser = null)
            => new ReplaceableParser<TInput, TOutput>(defaultParser ?? new FailParser<TInput, TOutput>());

        public static IParser<TInput, TOutput> Sequential<TOutput>(Func<SequentialState<TInput>, TOutput> func)
            => new SequentialParser<TInput, TOutput>(func);

        /// <summary>
        /// Transform one node into another node to fit into the grammar
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
    }
}
