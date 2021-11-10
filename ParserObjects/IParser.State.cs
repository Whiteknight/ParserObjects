using System;
using System.Collections.Generic;

namespace ParserObjects
{
    public static class ParserStateExtensions
    {
        /// <summary>
        /// The result value of the parser is stored as contextual state data in the parse state.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SetResultData<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
            => ParserMethods<TInput>.SetResultData(p, name);

        /// <summary>
        /// The result value of the parser is stored as contextual state data in the parse state.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SetResultData<TInput, TOutput, TValue>(this IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
            => ParserMethods<TInput>.SetResultData(p, name, getValue);

        /// <summary>
        /// Push a recursive data frame before executing the given parser, and then pop the data
        /// frame when the parser completes.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput>(this IParser<TInput, TOutput> p)
            => ParserMethods<TInput>.DataContext(p);

        public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IParser<TInput, TOutput> p, string name, TData value)
            where TData : notnull
            => ParserMethods<TInput>.DataContext(p, name, value);

        public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IParser<TInput, TOutput> p, Dictionary<string, TData> values)
            => ParserMethods<TInput>.DataContext(p, values);

        public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput>(this IMultiParser<TInput, TOutput> p)
            => ParserMethods<TInput>.DataContext(p);

        public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IMultiParser<TInput, TOutput> p, string name, TData value)
            where TData : notnull
            => ParserMethods<TInput>.DataContext(p, name, value);

        public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IMultiParser<TInput, TOutput> p, Dictionary<string, TData> values)
            => ParserMethods<TInput>.DataContext(p, values);
    }
}
