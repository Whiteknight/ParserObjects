using System;
using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// Base interface for results returned from a parse operation. The result should include
/// reference to the Parser which generated the reuslt and an indicator of success or failure.
/// Subclasses of this type will include more detailed information about the result.
/// </summary>
public interface IResultBase
{
    /// <summary>
    /// Gets the parser which created this result. Notice that this might not be the parser on
    /// which the Parse method was called, but may instead be some internal parser to which the
    /// task was delegated.
    /// </summary>
    IParser Parser { get; }

    /// <summary>
    /// Gets a value indicating whether the parse succeeded.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Try to get attached data with the given type. If none exists, returns failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Option<T> TryGetData<T>();
}

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
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    IResultAlternative<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform);
}

/// <summary>
/// The output of an IMultiParser which holds 0 or more result values.
/// </summary>
public interface IMultResult : IResultBase
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
public interface IMultResult<TOutput> : IMultResult
{
    /// <summary>
    /// Gets the list of typed result alternatives.
    /// </summary>
    new IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

    /// <summary>
    /// Create a new IMultResult of the same type, by transforming the data of the current
    /// instance.
    /// </summary>
    /// <param name="recreate"></param>
    /// <param name="parser"></param>
    /// <param name="startCheckpoint"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    IMultResult<TOutput> Recreate(
        CreateNewResultAlternative<TOutput> recreate,
        IParser? parser = null,
        SequenceCheckpoint? startCheckpoint = null,
        Location? location = null
    );

    /// <summary>
    /// Create a new IMultResult by applying a transformation to every alternative value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    IMultResult<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform);
}

public static class MultResultExtensions
{
    /// <summary>
    /// Select one of the result alternatives to turn into a single Result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="result"></param>
    /// <param name="alt"></param>
    /// <returns></returns>
    public static Result<TOutput> ToResult<TOutput>(this IMultResult<TOutput> result, IResultAlternative<TOutput> alt)
    {
        if (alt.Success)
            return Result<TOutput>.Ok(result.Parser, alt.Value, alt.Consumed);
        return Result<TOutput>.Fail(result.Parser, alt.ErrorMessage);
    }
}
