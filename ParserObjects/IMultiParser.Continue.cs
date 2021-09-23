﻿using ParserObjects.Parsers.Multi;

namespace ParserObjects
{
    public static partial class MultiParserExtensions
    {
        // Continue the parse with each alternative separately, and return a new multi-result with
        // the new results.
        public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, ContinueWith.SingleParserSelector<TInput, TMiddle, TOutput> getParser)
            => new ContinueWith.SingleParser<TInput, TMiddle, TOutput>(multiParser, getParser);

        public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, ContinueWith.MultiParserSelector<TInput, TMiddle, TOutput> getParser)
            => new ContinueWith.MultiParser<TInput, TMiddle, TOutput>(multiParser, getParser);
    }
}