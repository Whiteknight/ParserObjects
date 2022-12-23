using System;

namespace ParserObjects;

/// <summary>
/// Result object from a Parse operation.
/// </summary>
public interface IResult : IResultBase
{
    /// <summary>
    /// Gets a description of the failure from this result, if this result is a failure. If
    /// the result is a success, this will not contain useful information.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Gets the value returned from the parser, if any.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets the number of input items consumed from the input sequence during the Parse
    /// operation.
    /// </summary>
    int Consumed { get; }

    /// <summary>
    /// Return a new result with an updated Consumed count.
    /// </summary>
    /// <param name="consumed"></param>
    /// <returns></returns>
    IResult AdjustConsumed(int consumed);
}

/// <summary>
/// Result object from a Parse operation.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IResult<out TValue> : IResult
{
    /// <summary>
    /// Gets the produced value from the successful parse. If Success is false, this accessor
    /// will throw an exception.
    /// </summary>
    new TValue Value { get; }

    /// <summary>
    /// Return a new result with an updated Consumed count.
    /// </summary>
    /// <param name="consumed"></param>
    /// <returns></returns>
    new IResult<TValue> AdjustConsumed(int consumed);
}

public static class ParseResultExtensions
{
    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T GetValueOrDefault<T>(this IResult<T> result, T defaultValue)
        => result.Success ? result.Value : defaultValue;

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="getDefaultValue"></param>
    /// <returns></returns>
    public static T GetValueOrDefault<T>(this IResult<T> result, Func<T> getDefaultValue)
        => result.Success ? result.Value : getDefaultValue();

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static object GetValueOrDefault(this IResult result, object defaultValue)
        => result.Success ? result.Value : defaultValue;
}
