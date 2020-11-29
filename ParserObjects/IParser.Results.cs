using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserResultsExtensions
    {
        public static IParser<TInput, TOutput> TransformError<TInput, TOutput>(this IParser<TInput, TOutput> parser, TransformResult<TInput, TOutput, TOutput>.Function transform)
            => new TransformResult<TInput, TOutput, TOutput>.Parser(parser, (t, r) =>
            {
                if (r.Success)
                    return r;
                return transform(t, r);
            });
    }
}
