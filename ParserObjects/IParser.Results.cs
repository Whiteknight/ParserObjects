using System;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserResultsExtensions
    {
        public static IParser<TInput, TOutput> TransformError<TInput, TOutput>(this IParser<TInput, TOutput> parser, Func<ParseState<TInput>, IResult<TOutput>, IResult<TOutput>> transform)
            => new TransformResultParser<TInput, TOutput>(parser, (t, r) =>
            {
                if (r.Success)
                    return r;
                return transform(t, r);
            });
    }
}
