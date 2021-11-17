using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class MultiParserExtensions
    {
        /// <summary>
        /// Continue the parse with each alternative separately.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, ContinueWith<TInput, TMiddle, TOutput>.SingleParserSelector getParser)
            => new ContinueWith<TInput, TMiddle, TOutput>.SingleParser(multiParser, getParser);

        /// <summary>
        /// Continue the parse with each alternative separately.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, ContinueWith<TInput, TMiddle, TOutput>.MultiParserSelector getParser)
            => new ContinueWith<TInput, TMiddle, TOutput>.MultiParser(multiParser, getParser);

        public static IMultiParser<TInput, TOutput> ContinueWithEach<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> parser, Func<IParser<TInput, TMiddle>, IEnumerable<IParser<TInput, TOutput>>> getParsers)
            => ContinueWith(parser, left => new EachParser<TInput, TOutput>(getParsers(left).ToArray()));

        /// <summary>
        /// Transform the values of all result alternatives.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IMultiParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, Func<TMiddle, TOutput> transform)
            => ParserMethods<TInput>.Transform(multiParser, transform);

        /// <summary>
        /// Transform the values of all result alternatives.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="multiParser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IMultiParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, Transform<TInput, TMiddle, TOutput>.MultiFunction transform)
            => ParserMethods<TInput>.TransformResult(multiParser, transform);
    }
}
