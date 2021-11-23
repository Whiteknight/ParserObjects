﻿namespace ParserObjects;

/// <summary>
/// State information about parse, including the input sequence, logging and contextual data.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IParseState<out TInput>
{
    /// <summary>
    /// Gets a contextual data store which parsers may access during the parse.
    /// </summary>
    IDataStore Data { get; }

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
    /// <param name="location"></param>
    /// <returns></returns>
    public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser parser, string error, Location location)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailResult<TOutput>(parser, location, error);
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, string error)
        => Fail(state, parser, error, state.Input.CurrentLocation);

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, string error, Location location)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailResult<TOutput>(parser, location, error);
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IResult Fail<TInput>(this IParseState<TInput> state, IParser<TInput> parser, string error, Location location)
    {
        state.Log(parser, "Failed with error " + error);
        return new FailResult<object>(parser, location, error);
    }

    /// <summary>
    /// Create a Failure result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResult Fail<TInput>(this IParseState<TInput> state, IParser<TInput> parser, string error)
        => Fail(state, parser, error, state.Input.CurrentLocation);

    /// <summary>
    /// Create a Success result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="output"></param>
    /// <param name="consumed"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IResult<TOutput> Success<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, TOutput output, int consumed, Location location)
    {
        state.Log(parser, "Succeeded");
        return new SuccessResult<TOutput>(parser, output, location, consumed);
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
    /// <returns></returns>
    public static IResult<TOutput> Success<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, TOutput output, int consumed)
        => Success(state, parser, output, consumed, state.Input.CurrentLocation);

    /// <summary>
    /// Create a Success result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="output"></param>
    /// <param name="consumed"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public static IResult<object> Success<TInput>(this IParseState<TInput> state, IParser<TInput> parser, object output, int consumed, Location location)
    {
        state.Log(parser, "Succeeded");
        return new SuccessResult<object>(parser, output, location, consumed);
    }

    /// <summary>
    /// Create a Success result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="output"></param>
    /// <param name="consumed"></param>
    /// <returns></returns>
    public static IResult<object> Success<TInput>(this IParseState<TInput> state, IParser<TInput> parser, object output, int consumed)
        => Success(state, parser, output, consumed, state.Input.CurrentLocation);

    /// <summary>
    /// Create a result from a partial result.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="state"></param>
    /// <param name="parser"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    public static IResult<TOutput> Result<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, PartialResult<TOutput> part)
    {
        if (part.Success)
            return Success(state, parser, part.Value!, part.Consumed, part.Location);
        return Fail(state, parser, part.ErrorMessage!, part.Location);
    }
}
