using System;

namespace ParserObjects
{
    public static class ParserStateExtensions
    {
        public static IParser<TInput, TOutput> SetResultData<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
            => ParserMethods<TInput>.SetResultData(p, name);

        public static IParser<TInput, TOutput> SetResultData<TInput, TOutput, TValue>(this IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
            => ParserMethods<TInput>.SetResultData(p, name, getValue);
    }
}
