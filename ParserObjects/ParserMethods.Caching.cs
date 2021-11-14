using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
        /// location will return the same result.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput> Cache(IParser<TInput> p)
            => new Cache.NoOutputParser<TInput>(p);

        /// <summary>
        /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
        /// location will return the same result.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Cache<TOutput>(IParser<TInput, TOutput> p)
            => new Cache.OutputParser<TInput, TOutput>(p);

        /// <summary>
        /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
        /// location will return the same result.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IMultiParser<TInput, TOutput> Cache<TOutput>(IMultiParser<TInput, TOutput> p)
            => new Cache.MultiParser<TInput, TOutput>(p);
    }
}
