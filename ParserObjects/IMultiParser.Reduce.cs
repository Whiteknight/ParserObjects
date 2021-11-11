﻿using System;
using System.Linq;
using ParserObjects.Parsers.Multi;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class MultiParserExtensions
    {
        // Expect a single result and return it. Failure if 0 or more than 1
        public static IParser<TInput, TOutput> Single<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => multiParser.Select((multiResult, success, fail) =>
            {
                if (multiResult.Results.Count == 1)
                    return success(multiResult.Results[0]);
                return fail();
            });

        // Return the successful result which has consumed the most input, failure if there are no
        // successful results
        public static IParser<TInput, TOutput> Longest<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => multiParser.Select((multiResult, success, fail) =>
            {
                var longest = multiResult.Results
                    .Where(r => r.Success)
                    .OrderByDescending(r => r.Consumed)
                    .FirstOrDefault();
                return longest != null ? success(longest) : fail();
            });

        // Select the first result which matches the predicate, failure if nothing matches
        public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser, Func<IResultAlternative<TOutput>, bool> predicate)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            return multiParser.Select((multiResult, success, fail) =>
            {
                var selected = multiResult.Results.FirstOrDefault(predicate);
                return selected != null ? success(selected) : fail();
            });
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
            => First(multiParser, r => r.Success);

        public static IParser<TInput, TOutput> Select<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiparser, SelectMultiAlternativeFunction<TOutput> select)
            => new SelectSingleResultParser<TInput, TOutput>(multiparser, select);
    }
}
