using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

public static partial class Parsers<TInput>
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
    public static IParser<TInput, TOutput> Context<TOutput>(IParser<TInput, TOutput> parser, Action<Function<TInput, TOutput>.SingleArguments> setup, Action<Function<TInput, TOutput>.SingleArguments> cleanup)
        => new Function<TInput, TOutput>.Parser(args =>
        {
            try
            {
                setup(args);
                return parser.Parse(args.State);
            }
            finally
            {
                cleanup(args);
            }
        }, string.Empty, new[] { parser });

    public static IMultiParser<TInput, TOutput> Context<TOutput>(IMultiParser<TInput, TOutput> parser, Action<IParseState<TInput>> setup, Action<IParseState<TInput>> cleanup)
        => new Function<TInput, TOutput>.MultiParser(args =>
        {
            var state = args.State;
            try
            {
                setup(state);
                return parser.Parse(state);
            }
            finally
            {
                cleanup(state);
            }
        }, string.Empty, new[] { parser });

    /// <summary>
    /// Create a new parser using information from the current parse context. This parser is
    /// not cached, but will also not be reported to visitors.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="create"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Create<TOutput>(Func<IParseState<TInput>, IParser<TInput, TOutput>> create)
        => new Create<TInput, TOutput>.Parser(create);

    /// <summary>
    /// Create a new multi parser using information from the current parse context. This parser
    /// is not cached, but will also not be reported to visitors.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="create"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> CreateMulti<TOutput>(Func<IParseState<TInput>, IMultiParser<TInput, TOutput>> create)
        => new Create<TInput, TOutput>.MultiParser(create);

    /// <summary>
    /// A parser which tries to get a value from current contextual data and return it as the
    /// result. Fails if the value does not exist.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IParser<TInput, TValue> GetData<TValue>(string name)
        => Function<TValue>(args =>
        {
            var result = args.Data.Get<TValue>(name);
            return result.Success ?
                args.Success(result.Value, args.Input.CurrentLocation) :
                args.Failure($"State data '{name}' with type does not exist", args.Input.CurrentLocation);
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
        => Function<TValue>(args =>
        {
            args.Data.Set(name, value);
            return args.Success(value, args.Input.CurrentLocation);
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
        => SetResultData(p, name, value => value);

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
        => new Function<TInput, TOutput>.Parser(args =>
        {
            var result = p.Parse(args.State);
            if (result.Success)
            {
                var value = getValue(result.Value);
                args.Data.Set(name, value);
            }

            return result;
        }, "SetResultData", new[] { p });

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="inner"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> DataContext<TOutput, TData>(IParser<TInput, TOutput> inner, Dictionary<string, TData> values)
        => Context(inner,
            state =>
            {
                (state.Data as CascadingKeyValueStore)?.PushFrame();
                foreach (var value in values)
                    state.Data.Set(value.Key, value.Value);
            },
            state => (state.Data as CascadingKeyValueStore)?.PopFrame()
        );

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="inner"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> DataContext<TOutput, TData>(IMultiParser<TInput, TOutput> inner, Dictionary<string, TData> values)
        => Context(inner,
            state =>
            {
                (state.Data as CascadingKeyValueStore)?.PushFrame();
                foreach (var value in values)
                    state.Data.Set(value.Key, value.Value);
            },
            state => (state.Data as CascadingKeyValueStore)?.PopFrame()
        );

    /// <summary>
    /// Creates a new contextual data frame to store data. Execute the inner parser. When the
    /// inner parser concludes, pop the data frame off the data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> DataContext<TOutput>(IParser<TInput, TOutput> inner)
        => Context(inner,
            state => (state.Data as CascadingKeyValueStore)?.PushFrame(),
            state => (state.Data as CascadingKeyValueStore)?.PopFrame()
        );

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> DataContext<TOutput>(IMultiParser<TInput, TOutput> inner)
        => Context(inner,
            state => (state.Data as CascadingKeyValueStore)?.PushFrame(),
            state => (state.Data as CascadingKeyValueStore)?.PopFrame()
        );

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
        where TData : notnull
        => DataContext(inner, new Dictionary<string, object> { { name, value } });

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="inner"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> DataContext<TOutput, TData>(IMultiParser<TInput, TOutput> inner, string name, TData value)
        where TData : notnull
        => DataContext(inner, new Dictionary<string, object> { { name, value! } });
}
