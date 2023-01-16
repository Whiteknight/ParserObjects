using System;
using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// State information about parse, including the input sequence, logging and contextual data.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IParseState<out TInput>
{
    /// <summary>
    /// Gets a contextual data store which parsers may access during the parse.
    /// </summary>
    DataStore Data { get; }

    /// <summary>
    /// Gets the current input sequence.
    /// </summary>
    ISequence<TInput> Input { get; }

    /// <summary>
    /// Gets the current result cache.
    /// </summary>
    IResultsCache Cache { get; }

    /// <summary>
    /// Logs a message for the user to help with debugging.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="message"></param>
    void Log(IParser parser, string message);

    /// <summary>
    /// Execute the given callback inside a data frame.
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="args"></param>
    /// <param name="withContext"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    TResult WithDataFrame<TArgs, TResult>(TArgs args, Func<IParseState<TInput>, TArgs, TResult> withContext, IReadOnlyDictionary<string, object>? data = null);
}

public static class ParseStateExtensions
{
    /// <summary>
    /// Create a failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser parser, string error, IReadOnlyList<object>? data = null)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailureResult<TOutput>(parser, error, new ResultData(data));
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, string error, IReadOnlyList<object>? data = null)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailureResult<TOutput>(parser, error, new ResultData(data));
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IResult Fail<TInput>(this IParseState<TInput> state, IParser<TInput> parser, string error, IReadOnlyList<object>? data = null)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailureResult<object>(parser, error, new ResultData(data));
    }

    /// <summary>
    /// Create a Success result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="output"></param>
    /// <param name="consumed"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IResult<TOutput> Success<TInput, TOutput>(this IParseState<TInput> state, IParser parser, TOutput output, int consumed, IReadOnlyList<object>? data = null)
    {
        state.Log(parser, "Succeeded");
        return new SuccessResult<TOutput>(parser, output, consumed, new ResultData(data));
    }

    /// <summary>
    /// Create a result from a partial result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    public static IResult<TOutput> Result<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput> parser, PartialResult<TOutput> part)
    {
        if (part.Success)
            return Success(state, parser, part.Value!, part.Consumed);
        return Fail<TInput, TOutput>(state, parser, part.ErrorMessage!);
    }
}
