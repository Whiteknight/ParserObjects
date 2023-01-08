using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Adjust the current parse context before a parse and cleanup any changes after the
    /// parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="setup"></param>
    /// <param name="cleanup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Context<TOutput>(IParser<TInput, TOutput> parser, Action<IParseState<TInput>>? setup, Action<IParseState<TInput>>? cleanup)
    {
        if (setup == null && cleanup == null)
            return parser;
        return new Context<TInput>.Parser<TOutput>(parser, setup, cleanup);
    }

    /// <summary>
    /// Adjust teh current parse context before a parse and cleanup any changes after the parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="setup"></param>
    /// <param name="cleanup"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Context<TOutput>(IMultiParser<TInput, TOutput> parser, Action<IParseState<TInput>> setup, Action<IParseState<TInput>> cleanup)
    {
        if (setup == null && cleanup == null)
            return parser;
        return new Context<TInput>.MultiParser<TOutput>(parser, setup, cleanup);
    }

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
        => new Function<TInput, TValue>.Parser<string>(
            name,
            static (state, n, args) =>
            {
                var result = state.Data.Get<TValue>(n);
                return result.Success ?
                    args.Success(result.Value) :
                    args.Failure($"State data '{n}' with type does not exist");
            },
            static (_, _) => true,
            $"GET '{name}'",
            Array.Empty<IParser>()
        );

    private readonly record struct SetDataArgs<TValue>(string Name, TValue Value);

    /// <summary>
    /// A parser which sets a value into the current context data and returns that same value
    /// as a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IParser<TInput, TValue> SetData<TValue>(string name, TValue value)
        => new Function<TInput, TValue>.Parser<SetDataArgs<TValue>>(
            new SetDataArgs<TValue>(name, value),
            static (state, sdArgs, funcArgs) =>
            {
                state.Data.Set(sdArgs.Name, sdArgs.Value);
                return funcArgs.Success(sdArgs.Value);
            },
            static (_, _) => true,
            $"SET '{name}'",
            Array.Empty<IParser>()
        );

    /// <summary>
    /// Execute the inner parser and, on success, save the result value to the current context
    /// data store with the given name.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> SetResultData<TOutput>(IParser<TInput, TOutput> p, string name)
        => SetResultData(p, name, static value => value);

    // This one has to stay public so Bnf stringifier .Accept() method can bind to it.
    public readonly record struct SetResultDataArgs<TOutput, TValue>(IParser<TInput, TOutput> Parser, string Name, Func<TOutput, TValue> GetValue);

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
        => new Function<TInput, TOutput>.Parser<SetResultDataArgs<TOutput, TValue>>(
            new SetResultDataArgs<TOutput, TValue>(p, name, getValue),
            static (state, srdArgs, _) =>
            {
                var result = srdArgs.Parser.Parse(state);
                if (result.Success)
                {
                    var value = srdArgs.GetValue(result.Value);
                    state.Data.Set(srdArgs.Name, value);
                }

                return result;
            },
            static (state, srdArgs) => srdArgs.Parser.Match(state),
            "SetResultData",
            new[] { p }
        );

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> DataContext<TOutput>(IParser<TInput, TOutput> inner, Dictionary<string, object> values)
        => new DataFrame<TInput>.Parser<TOutput>(inner, values);

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> DataContext<TOutput>(IMultiParser<TInput, TOutput> inner, Dictionary<string, object> values)
        => new DataFrame<TInput>.MultiParser<TOutput>(inner, values);

    /// <summary>
    /// Creates a new contextual data frame to store data. Execute the inner parser. When the
    /// inner parser concludes, pop the data frame off the data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> DataContext<TOutput>(IParser<TInput, TOutput> inner)
        => new DataFrame<TInput>.Parser<TOutput>(inner);

    /// <summary>
    /// Creates a new contextual data frame to store data if the data store supports frames.
    /// Execute the inner parser. When the inner parser concludes, pop the data frame off the
    /// data store.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> DataContext<TOutput>(IMultiParser<TInput, TOutput> inner)
        => new DataFrame<TInput>.MultiParser<TOutput>(inner);

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
