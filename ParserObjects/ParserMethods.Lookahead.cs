using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Zero-length assertion that the given pattern does not match from the current position. No
        /// input is consumed
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, object> NegativeLookahead(IParser<TInput> p)
            => new NegativeLookaheadParser<TInput>(p);

        /// <summary>
        /// Zero-length assertion that the given pattern matches from the current position. No input is
        /// consumed.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> PositiveLookahead<TOutput>(IParser<TInput, TOutput> p)
            => new PositiveLookaheadParser<TInput, TOutput>(p);
    }
}
