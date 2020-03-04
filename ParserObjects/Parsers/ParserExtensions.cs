using System;
using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// IParser extension methods for building combinators using fluent syntax
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Flattens the enumerable result of a given parser.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Flatten<TInput, TCollection, TOutput>(this IParser<TInput, TCollection> parser)
            where TCollection : IEnumerable<TOutput>
            => new FlattenParser<TInput, TCollection, TOutput>(parser);

        /// <summary>
        /// Zero-length assertion that the given parser's result is followed by another sequence.
        /// The lookahead sequence is matched but not consumed
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> FollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
            => Rule(p, PositiveLookahead(lookahead), (result, match) => result);

        /// <summary>
        /// Returns a list of results from the given parser. Continues to parse until the parser returns
        /// failure. Returns an enumerable of results.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, IEnumerable<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, bool atLeastOne = false) 
            => new ListParser<TInput, TOutput>(p, atLeastOne);

        /// <summary>
        /// Given a parser which parses characters, parse a list of characters and return the sequence as a
        /// string
        /// </summary>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<char, string> ListCharToString(this IParser<char, char> p, bool atLeastOne = false)
            => p.List(atLeastOne).Transform(c => new string(c.ToArray()));

        /// <summary>
        /// Returns a list of results from the given parser separated by a separator pattern. Continues until
        /// the item or separator parser return failure. Returns an enumerable of results.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, IEnumerable<TOutput>> ListSeparatedBy<TInput, TSeparator, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput, TSeparator> separator, bool atLeastOne = false)
            => SeparatedList(p, separator, atLeastOne);

        /// <summary>
        /// Given a parser which parses strings, parse a list of strings and return the sequence as a joined
        /// string
        /// </summary>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<char, string> ListStringsToString(this IParser<char, string> p, bool atLeastOne = false)
            => p.List(atLeastOne).Transform(s => string.Join(string.Empty, s));

        /// <summary>
        /// Zero-length assertion that the given parser's match result is not followed by a lookahead pattern.
        /// The lookahead is compared but no input is consumed to match it.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> NotFollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
            => Rule(p, NegativeLookahead(lookahead), (result, match) => result);

        /// <summary>
        /// The results of the given parser are optional. If the given parser fails, a default value will
        /// be provided 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
            => First(p, Produce<TInput, TOutput>(getDefault ?? (() => default)));

        /// <summary>
        /// The results of the given parser are optiona. If the given parser fails, a default value will be
        /// provided
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> produce)
            => First(p, Produce(produce ?? (t => default)));

        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IParser<TInput, TOutput> p)
            => new ReplaceableParser<TInput, TOutput>(p);

        /// <summary>
        /// Transform the output of the given parser to a new value 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform) 
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
    }
}