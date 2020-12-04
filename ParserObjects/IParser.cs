using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Parsers.Multi;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects
{
    // marker interface for items which may act as a symbol in a grammar
    public interface ISymbol : INamed
    {
    }

    // marker interface for items which may act as a symbol in a grammar with a specific output
    // type.
    public interface ISymbol<out TValue> : ISymbol
    {
    }

    /// <summary>
    /// Parser base type.
    /// </summary>
    public interface IParser : ISymbol
    {
        /// <summary>
        /// Get a list of child parsers, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IParser> GetChildren();
    }

    /// <summary>
    /// Parser object which allows getting the result without type information.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IParser<TInput> : IParser
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result of type object. If the
        /// parse fails, it is expected that this method will return the input sequence to the
        /// state it was at before the parse was attempted.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        IResult Parse(IParseState<TInput> state);
    }

    /// <summary>
    /// Parser with explicit input and output types.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParser<TInput, out TOutput> : IParser<TInput>, ISymbol<TOutput>
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result. If the parse fails,
        /// it is expected that this method will return the input sequence to the state it was at
        /// before the parse was attempted.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        new IResult<TOutput> Parse(IParseState<TInput> state);
    }

    /// <summary>
    /// A parser which has an in-place replaceable child. Used to identify parsers which can
    /// participate in certain find/replace operations.
    /// </summary>
    public interface IReplaceableParserUntyped : IParser
    {
        /// <summary>
        /// Gets the child parser which can be replaced without cloning.
        /// </summary>
        IParser ReplaceableChild { get; }

        /// <summary>
        /// Set the new child parser without cloning.
        /// </summary>
        /// <param name="parser"></param>
        SingleReplaceResult SetParser(IParser parser);
    }

    // TODO: Make sure this inheriting from IParser doesn't imply it can be mixed in with other
    // combinators. May need a new interface or some guards or something. Don't want this getting
    // dumped into RuleParser or anything like that
    public interface IMultiParser<TInput, TOutput> : IParser
    {
        // MultiParser should return the input to the starting location, because every result
        // has a .Continuation checkpoint which can be invoked, and an error result might be used
        // for continuation.
        IMultiResult<TOutput> Parse(IParseState<TInput> state);
    }

    /*
     * Expect there to be a single result and return it. Ambiguity failure if more than one
     *      Converts IMultiParser->IParser
     * var result = Earley(...).Single();
     *
     * Select the "best" result to convert IMultiParser->IParser
     * var result = Earley(...).First(multiResultAlt => ...)
     *
     * Use each result in turn to test a possible continuation. IMultiResult<T1> -> IMultiResult<T2>
     * var multiResult = Earley(...).ForEach(left => Rule(left, ...));
     */

    public static class MultiParserExtensions
    {
        public static IMultiResult<TOutput> Parse<TOutput>(this IMultiParser<char, TOutput> p, string s)
            => p.Parse(new ParseState<char>(new StringCharacterSequence(s), Defaults.LogMethod));

        // Expect a single result and return it. Failure if 0 or more than 1
        public static IParser<TInput, TOutput> Single<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => new SelectSingleResultParser<TInput, TOutput>(multiParser, multiResult =>
            {
                if (multiResult.Results.Count == 1)
                    return new SuccessOption<IMultiResultAlternative<TOutput>>(multiResult.Results.First());

                // TODO: Would like to differentiate between Count==0 which is no results and
                // Count>1 which is ambiguous
                return FailureOption<IMultiResultAlternative<TOutput>>.Instance;
            });

        // Return the successful result which has consumed the most input, failure if there are no
        // successful results
        public static IParser<TInput, TOutput> Longest<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => new SelectSingleResultParser<TInput, TOutput>(multiParser, multiResult =>
            {
                var longest = multiResult.Results.Where(r => r.Success).OrderByDescending(r => r.Consumed).FirstOrDefault();
                if (longest == null)
                    return FailureOption<IMultiResultAlternative<TOutput>>.Instance;

                return new SuccessOption<IMultiResultAlternative<TOutput>>(longest);
            });

        // Select the first result which matches the predicate, failure if nothing matches
        public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser, Func<IMultiResultAlternative<TOutput>, bool> predicate)
            => new SelectSingleResultParser<TInput, TOutput>(multiParser, multiResult =>
            {
                var selected = multiResult.Results.Where(predicate).FirstOrDefault();
                if (selected == null)
                    return FailureOption<IMultiResultAlternative<TOutput>>.Instance;

                return new SuccessOption<IMultiResultAlternative<TOutput>>(selected);
            });

        // Continue the parse with each alternative separately, and return a new multi-result with
        // the new results.
        public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, Func<IParser<TInput, TMiddle>, IParser<TInput, TOutput>> getParser)
            => new ContinueWithParser<TInput, TMiddle, TOutput>(multiParser, getParser);
    }
}
