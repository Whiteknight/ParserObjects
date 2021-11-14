using System;
using System.Linq;
using ParserObjects.Parsers.Multi;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class MultiParserExtensions
    {
        /// <summary>
        /// Expect the IMultiResult to contain exactly 1 alternative, and select that to continue.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Single<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => multiParser.Select((multiResult, success, fail) =>
            {
                if (multiResult.Results.Count == 1)
                    return success(multiResult.Results[0]);
                return fail();
            });

        /// <summary>
        /// Select the result alternative which consumed the most amount of input and use that to
        /// continue the parse. If there are no alternatives, returns failure. If there are ties,
        /// the first is selected.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Longest<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => multiParser.Select((multiResult, success, fail) =>
            {
                var longest = multiResult.Results
                    .Where(r => r.Success)
                    .OrderByDescending(r => r.Consumed)
                    .FirstOrDefault();
                return longest != null ? success(longest) : fail();
            });

        /// <summary>
        /// Returns the first successful alternative which matches a predicate to continue the
        /// parse with.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser, Func<IResultAlternative<TOutput>, bool> predicate)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            return multiParser.Select((multiResult, success, fail) =>
            {
                var selected = multiResult.Results.FirstOrDefault(predicate);
                return selected != null ? success(selected) : fail();
            });
        }

        /// <summary>
        /// Selects the first successful alternative to continue the parse with.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => First(multiParser, r => r.Success);

        /// <summary>
        /// Invoke a special callback to attempt to select a single alternative and turn it into
        /// an IResult.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiparser"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Select<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiparser, Select<TInput, TOutput>.Function select)
            => new Select<TInput, TOutput>.Parser(multiparser, select);
    }
}
