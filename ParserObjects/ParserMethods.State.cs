using System;
using System.Collections.Generic;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Adjust the current parse context before a parse, and cleanup any changes after the
        /// parse.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="setup"></param>
        /// <param name="cleanup"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Context<TOutput>(IParser<TInput, TOutput> parser, Context<TInput, TOutput>.Function setup, Context<TInput, TOutput>.Function cleanup)
            => new Context<TInput, TOutput>.Parser(parser, setup, cleanup);

        /// <summary>
        /// Create a new parser using information from the current parse context. This parser is
        /// not cached, but will also not be reported to visitors.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="create"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Create<TOutput>(Create<TInput, TOutput>.Function create)
            => new Create<TInput, TOutput>.Parser(create);

        /// <summary>
        /// A parser which tries to get a value from current contextual data and return it as the
        /// result. Fails if the value does not exist.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser<TInput, TValue> GetData<TValue>(string name)
            => Function<TValue>((t, success, fail) =>
            {
                var result = t.Data.Get<TValue>(name);
                return result.Success ? success(result.Value) : fail($"State data '{name}' does not exist");
            });

        /// <summary>
        /// A parser which sets a value into the current context data and returns that same value
        /// as a result.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IParser<TInput, TValue> SetData<TValue>(string name, TValue value)
            => Function<TValue>((t, success, _) =>
            {
                t.Data.Set(name, value);
                return success(value);
            });

        /// <summary>
        /// Execute the inner parser and, on success, save the result value to the current context
        /// data store with the given name.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SetResultData<TOutput>(IParser<TInput, TOutput> p, string name)
            => TransformResult(p, (_, data, result) =>
            {
                if (result.Success)
                    data.Set(name, result.Value);

                return result;
            });

        /// <summary>
        /// Execute the inner parser and, on success, save the result value (possibly transformed
        /// using the given transformation function) to the current context data store with the
        /// given name.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SetResultData<TOutput, TValue>(IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
            => TransformResult(p, (_, data, result) =>
            {
                if (result.Success)
                {
                    var value = getValue(result.Value);
                    data.Set(name, value);
                }

                return result;
            });

        /// <summary>
        /// Creates a new contextual data frame to store data. Execute the inner parser. When the
        /// inner parser concludes, pop the data frame off the data store.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="inner"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> DataContext<TOutput, TData>(IParser<TInput, TOutput> inner, Dictionary<string, TData>? values)
            => Context(inner,
                (_, d) =>
                {
                    (d as CascadingKeyValueStore)?.PushFrame();
                    if (values != null)
                    {
                        foreach (var value in values)
                            d.Set(value.Key, value.Value);
                    }
                },
                (_, d) => (d as CascadingKeyValueStore)?.PopFrame()
            );

        /// <summary>
        /// Creates a new contextual data frame to store data. Execute the inner parser. When the
        /// inner parser concludes, pop the data fram off the data store.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> DataContext<TOutput>(IParser<TInput, TOutput> inner)
            => DataContext<TOutput, object>(inner, null);

        /// <summary>
        /// Creates a new contextual data frame to store data, populated initially with the given
        /// data value. Execute the inner parser. when the inner parser concludes, pop the data
        /// frame off the data store.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="inner"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> DataContext<TOutput, TData>(IParser<TInput, TOutput> inner, string name, TData value)
            => DataContext(inner, new Dictionary<string, object> { { name, value } });
    }
}
