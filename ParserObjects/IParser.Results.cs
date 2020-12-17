using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserResultsExtensions
    {
        /// <summary>
        /// If the result is an error, invoke the callback to modify the result and result
        /// metadata. If the original result is success, return it without modification.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> TransformError<TInput, TOutput>(this IParser<TInput, TOutput> parser, Transform<TInput, TOutput, TOutput>.Function transform)
            => new Transform<TInput, TOutput, TOutput>.Parser(parser, (t, d, r) =>
            {
                if (r.Success)
                    return r;
                return transform(t, d, r);
            });
    }
}
