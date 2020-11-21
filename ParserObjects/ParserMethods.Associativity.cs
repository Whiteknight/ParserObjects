using System;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// A left-associative parser where the left item is parsed unconditionally, and the result of the
        /// left parser is applied to the right parser. This new result is then treated as the 'left' value
        /// for the next iteration of the right parser. This can be used when many rules have a common prefix
        /// and you don't want to backtrack through the prefix on every attempt.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="left"></param>
        /// <param name="getRight"></param>
        /// <param name="arity"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> LeftApply<TOutput>(IParser<TInput, TOutput> left, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> getRight, ApplyArity arity = ApplyArity.ZeroOrMore)
            => new LeftApplyParser<TInput, TOutput>(left, getRight, arity);

        /// <summary>
        /// a right-associative parser where the parser attempts to parse a sequence of items and middles
        /// recursively: self := &lt;item&gt; (&lt;middle&gt; &lt;self&gt;)*. 
        /// </summary>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="item"></param>
        /// <param name="middle"></param>
        /// <param name="produce"></param>
        /// <param name="getMissingRight"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> RightApply<TMiddle, TOutput>(IParser<TInput, TOutput> item, IParser<TInput, TMiddle> middle, Func<TOutput, TMiddle, TOutput, TOutput> produce, Func<ISequence<TInput>, TOutput> getMissingRight = null)
            => new RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput>(item, middle, produce, getMissingRight);
    }
}
