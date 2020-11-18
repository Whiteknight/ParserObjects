using System;

namespace ParserObjects
{
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
        TValue Value { get; }

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
        /// Transforms the Value of the result to type object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResult<object> Untype<T>(this IResult<T> result)
            => result.Transform(x => (object)x);

        public static IResult<T> WithError<T>(this IResult<T> result, string error)
            => new Result<T>(result.Parser, result.Success, result.Value, result.Location, error);

        public static IResult<T> WithError<T>(this IResult<T> result, Func<string, string> mutateError)
            => new Result<T>(result.Parser, result.Success, result.Value, result.Location, mutateError?.Invoke(result.Message) ?? result.Message);
    }
}