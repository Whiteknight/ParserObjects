using System;
using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// State information about parse, including the input sequence, logging and contextual data.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IParseState<TInput>
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
    /// Create a failure result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result<TOutput> Fail<TOutput>(
        IParser parser,
        string error,
        ResultData data = default
    )
    {
        Log(parser, "Failed with error " + error);
        return ParserObjects.Result<TOutput>.Fail(parser, error) with { Data = data };
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result<TOutput> Fail<TOutput>(
        IParser<TInput, TOutput> parser,
        string error,
        ResultData data = default
    )
    {
        Log(parser, "Failed with error " + error);
        return ParserObjects.Result<TOutput>.Fail(parser, error) with { Data = data };
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result<object> Fail(
        IParser<TInput> parser,
        string error,
        ResultData data = default
    )
    {
        Log(parser, "Failed with error " + error);
        return ParserObjects.Result<object>.Fail(parser, error) with { Data = data };
    }

    /// <summary>
    /// Create a Success result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="output"></param>
    /// <param name="consumed"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result<TOutput> Success<TOutput>(
        IParser parser,
        TOutput output,
        int consumed,
        ResultData data = default
    )
    {
        Log(parser, "Succeeded");
        return ParserObjects.Result<TOutput>.Ok(parser, output, consumed) with { Data = data };
    }

    /// <summary>
    /// Create a result from a partial result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    public Result<TOutput> Result<TOutput>(
        IParser<TInput> parser,
        PartialResult<TOutput> part
    ) => part.ToResult(parser);
}
