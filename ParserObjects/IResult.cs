using System;

namespace ParserObjects
{
    /// <summary>
    /// Result object from a Parse operation
    /// </summary>
    public interface IResult
    {
        IParser Parser { get; }

        /// <summary>
        /// Returns true if the parse succeeded, false otherwise.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// The approximate location of the successful parse in the input sequence. On failure, this
        /// value is undefined and may show the location of the start of the attempt, the location at
        /// which failure occured, null, or some other value.
        /// </summary>
        Location Location { get; }

        string Message { get; }

        object Value { get; }
    }

    /// <summary>
    /// Result object from a Parse operation
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IResult<out TValue> : IResult
    {
        /// <summary>
        /// The produced value from the successful parse. If Success is false, this value is undefined.
        /// </summary>
        new TValue Value { get; }

        /// <summary>
        /// Transforms the Value of the result to a new form
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform);
    }

    public static class ParseResultExtensions
    {
        /// <summary>
        /// Create a copy of the result, with a new error message. If the original result is
        /// Success, the result is returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IResult<T> WithError<T>(this IResult<T> result, string error)
            => result.Success ? result : new Result<T>(result.Parser, result.Success, result.Value, result.Location, error);

        /// <summary>
        /// Create a copy of the result with a modified error message. If the original result is
        /// Success, the result is returned unmodified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="mutateError"></param>
        /// <returns></returns>
        public static IResult<T> WithError<T>(this IResult<T> result, Func<string, string> mutateError)
            => result.Success ? result : new Result<T>(result.Parser, result.Success, result.Value, result.Location, mutateError?.Invoke(result.Message) ?? result.Message);

        /// <summary>
        /// Get the success flag and, if success is true, the result value
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="result"></param>
        /// <param name="success"></param>
        /// <param name="value"></param>
        public static void Deconstruct<TValue>(this IResult<TValue> result, out bool success, out TValue value)
        {
            success = result.Success;
            value = result.Value;
        }

        /// <summary>
        /// Get the success flag and location and, if success is true, the result value
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="result"></param>
        /// <param name="success"></param>
        /// <param name="value"></param>
        /// <param name="location"></param>
        public static void Deconstruct<TValue>(this IResult<TValue> result, out bool success, out TValue value, out Location location)
        {
            success = result.Success;
            value = result.Value;
            location = result.Location;
        }
    }
}