using System;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserStateExtensions
    {
        public static IParser<TInput, TOutput> SetResultState<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
            => new TransformResultParser<TInput, TOutput>(p, (t, r) =>
            {
                if (!r.Success)
                    return r;

                t.Data.Set(name, r.Value);
                return r;
            });

        public static IParser<TInput, TOutput> SetResultState<TInput, TOutput, TValue>(this IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
            => new TransformResultParser<TInput, TOutput>(p, (t, r) =>
            {
                if (!r.Success)
                    return r;

                var value = getValue(r.Value);
                t.Data.Set(name, value);
                return r;
            });
    }
}
