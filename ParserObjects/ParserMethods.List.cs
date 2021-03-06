﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Parse a list of items.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="atLeastOne">If true, the list must have at least one element or the parse fails. If
        /// false, an empty list returns success.</param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, bool atLeastOne)
            => new LimitedListParser<TInput, TOutput>(p, atLeastOne ? 1 : 0, null);

        /// <summary>
        /// Parse a list of items with defined minimum and maximum quantities.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, int minimum = 0, int? maximum = null)
            => new LimitedListParser<TInput, TOutput>(p, minimum, maximum);

        /// <summary>
        /// Parse a list of items separated by a separator pattern.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="atLeastOne">True if the list must contain at least one element or failure. False
        /// if an empty list can be returned.</param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> SeparatedList<TOutput>(IParser<TInput, TOutput> p, IParser<TInput> separator, bool atLeastOne)
            => SeparatedList(p, separator, atLeastOne ? 1 : 0, null);

        /// <summary>
        /// Parse a list of items separated by a separator pattern, with minimum and
        /// maximum item counts.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> SeparatedList<TOutput>(IParser<TInput, TOutput> p, IParser<TInput> separator, int minimum = 0, int? maximum = null)
        {
            var atLeastOneItemList = Rule(
                p,
                List(
                    Combine(
                        separator,
                        p
                    ).Transform(r => (TOutput)r[1]),
                    minimum - 1,
                    maximum - 1
                ),
                (first, rest) => (IReadOnlyList<TOutput>)new[] { first }.Concat(rest).ToList()
            );

            if (minimum >= 1)
            {
                // <p> (<separator> <p>)*
                return atLeastOneItemList;
            }

            // (<p> (<separator> <p>)*) | empty
            return First(
                atLeastOneItemList,
                Produce<IReadOnlyList<TOutput>>(() => new List<TOutput>())
            );
        }
    }
}
