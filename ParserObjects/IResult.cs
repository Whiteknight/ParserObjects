using System;

namespace ParserObjects
{
    /// <summary>
    /// Result object from a Parse operation.
    /// </summary>
    public interface IResult
    {
        IParser Parser { get; }

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

        string Message { get; }

        object Value { get; }

        int Consumed { get; }
    }

    /// <summary>
    /// Result object from a Parse operation.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IResult<out TValue> : IResult
    {
        /// <summary>
        /// Gets the produced value from the successful parse. If Success is false, this value is undefined.
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

    public interface IPartialResult<T>
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

        string Message { get; }

        T Value { get; }

        int Consumed { get; }
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
            => result.Success ? result : new FailResult<T>(result.Parser, result.Location, mutateError?.Invoke(result.Message) ?? result.Message);
    }
}
