using System;

namespace ParserObjects
{
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
        /// Transforms the Value of the result to a new form.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform);
    }

    /// <summary>
    /// Like an IResult but doesn't contain a reference to a Parser and some of the values may not
    /// be filled in completely. This is used internally by some parsers to communicate partial
    /// or incomplete results between components.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPartialResult<out T>
    {
        /// <summary>
        /// Gets a value indicating whether the parse succeeded.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Gets the approximate location of the successful parse in the input sequence. On failure, this
        /// value is undefined and may show the location of the start of the attempt, the location at
        /// which failure occured, null, or some other value.
        /// </summary>
        Location Location { get; }

        /// <summary>
        /// Gets a text description of the failure, if the result is not success.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets the result value of the operation, if any.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Gets the number of input items consumed during the operation.
        /// </summary>
        int Consumed { get; }
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

        /// <summary>
        /// Create a copy of the result, with a new error message. If the original result is
        /// Success, the result is returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IResult<T> WithError<T>(this IResult<T> result, string error)
            => result.Success ? result : new FailResult<T>(result.Parser, result.Location, error);

        /// <summary>
        /// Create a copy of the result with a modified error message. If the original result is
        /// Success, the result is returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="mutateError"></param>
        /// <returns></returns>
        public static IResult<T> WithError<T>(this IResult<T> result, Func<string, string> mutateError)
            => result.Success ? result : new FailResult<T>(result.Parser, result.Location, mutateError?.Invoke(result.ErrorMessage) ?? result.ErrorMessage);
    }
}
