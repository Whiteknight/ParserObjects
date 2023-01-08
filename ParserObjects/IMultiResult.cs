using System;
using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// One of several possible results from an IMultiParser.
/// </summary>
public interface IResultAlternative
{
    /// <summary>
    /// Gets a value indicating whether the alternative attempt succeeded or failed.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Gets the error message if the alternative is a failure.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Gets the result value, if the alternative is a success. Throws an exception otherwise.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets the number of input items consumed by this attempt.
    /// </summary>
    int Consumed { get; }

    /// <summary>
    /// Gets the sequence checkpoint from which to continue the parse if this alternate is
    /// selected.
    /// </summary>
    SequenceCheckpoint Continuation { get; }
}

/// <summary>
/// Factory method for creating a new result alternative of the same type.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="value"></param>
/// <param name="consumed"></param>
/// <param name="continuation"></param>
/// <returns></returns>
public delegate IResultAlternative<TOutput> ResultAlternativeFactoryMethod<TOutput>(TOutput value, int consumed, SequenceCheckpoint continuation);

/// <summary>
/// A result alternative with typed output.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public interface IResultAlternative<TOutput> : IResultAlternative
{
    /// <summary>
    /// Gets the value of the alternative if successful. On failure throws an exception.
    /// </summary>
    new TOutput Value { get; }

    /// <summary>
    /// Gets a factory which can create new results of this same type.
    /// </summary>
    ResultAlternativeFactoryMethod<TOutput> Factory { get; }

    /// <summary>
    /// Creates a new result with a transformed value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    IResultAlternative<TValue> Transform<TValue>(Func<TOutput, TValue> transform);
}

/// <summary>
/// The output of an IMultiParser which holds 0 or more result values.
/// </summary>
public interface IMultiResult : IResultBase
{
    /// <summary>
    /// Gets the sequence checkpoint from the start of the attempt.
    /// </summary>
    SequenceCheckpoint StartCheckpoint { get; }

    /// <summary>
    /// Gets the list of result alternatives.
    /// </summary>
    IReadOnlyList<IResultAlternative> Results { get; }
}

/// <summary>
/// The output of an IMultiParser which holds 0 or more typed result values.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public interface IMultiResult<TOutput> : IMultiResult
{
    /// <summary>
    /// Gets the list of typed result alternatives.
    /// </summary>
    new IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

    /// <summary>
    /// Create a new IMultiResult of the same type, by transforming the data of the current
    /// instance.
    /// </summary>
    /// <param name="recreate"></param>
    /// <param name="parser"></param>
    /// <param name="startCheckpoint"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    IMultiResult<TOutput> Recreate(Func<IResultAlternative<TOutput>, ResultAlternativeFactoryMethod<TOutput>, IResultAlternative<TOutput>> recreate, IParser? parser = null, SequenceCheckpoint? startCheckpoint = null, Location? location = null);

    /// <summary>
    /// Create a new IMultiResult by applying a transformation to every alternative value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    IMultiResult<TValue> Transform<TValue>(Func<TOutput, TValue> transform);
}

public static class MultiResultExtensions
{
    /// <summary>
    /// Select one of the result alternatives to turn into a single IResult.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="result"></param>
    /// <param name="alt"></param>
    /// <returns></returns>
    public static IResult<TOutput> ToResult<TOutput>(this IMultiResult<TOutput> result, IResultAlternative<TOutput> alt)
    {
        if (alt.Success)
            return new SuccessResult<TOutput>(result.Parser, alt.Value, alt.Consumed, default);
        return new FailureResult<TOutput>(result.Parser, alt.ErrorMessage, default);
    }
}
