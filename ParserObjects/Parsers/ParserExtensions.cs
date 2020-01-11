using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public static class ParserExtensions
    {
        public static IParser<TInput, TOutput> List<TInput, TItem, TOutput>(this IParser<TInput, TItem> p, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false) 
            => new ListParser<TInput, TItem, TOutput>(p, produce, atLeastOne);

        public static IParser<char, string> ListCharToString(this IParser<char, char> p, bool atLeastOne = false)
            => new ListParser<char, char, string>(p, c => new string(c.ToArray()), atLeastOne);

        public static IParser<char, string> ListStringsToString(this IParser<char, string> p, bool atLeastOne = false)
            => new ListParser<char, string, string>(p, s => string.Join("", s), atLeastOne);

        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TOutput> getDefault = null) 
            => new OptionalParser<TInput, TOutput>(p, getDefault);

        public static IParser<TInput, TOutput> Or<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> produce) 
            => new RequiredParser<TInput, TOutput>(p, produce);

        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform) 
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
    }
}