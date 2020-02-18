using System;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Logical
{
    public static class ParserMethods
    {
        public static IParser<TInput, bool> And<TInput>(IParser<TInput, bool> p1, IParser<TInput, bool> p2)
            => new AndParser<TInput>(p1, p2);

        /// <summary>
        /// Invokes the parser if the predicate is satisifed
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TInput, TOutput>(IParser<TInput, bool> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser);

        public static IParser<TInput, TOutput> If<TInput, TOutput>(Func<ISequence<TInput>, bool> predicate, IParser<TInput, TOutput> parser)
            => If(Produce(predicate), parser);

        public static IParser<TInput, TOutput> If<TInput, TOutput>(Func<bool> predicate, IParser<TInput, TOutput> parser)
            => If(Produce<TInput, bool>(predicate), parser);

        public static IParser<TInput, TOutput> IfLookahead<TInput, TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => If(PositiveLookahead(predicate), parser);

        public static IParser<TInput, TOutput> IfNotLookahead<TInput, TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => If(NegativeLookahead(predicate), parser);

        public static IParser<TInput, bool> Not<TInput>(IParser<TInput, bool> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput, bool> Or<TInput>(IParser<TInput, bool> p1, IParser<TInput, bool> p2)
            => new OrParser<TInput>(p1, p2);
    }
}