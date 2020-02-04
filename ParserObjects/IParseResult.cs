using System;

namespace ParserObjects
{
    /// <summary>
    /// Result object from a Parse operation
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IParseResult<out TValue>
    {
        /// <summary>
        /// Returns true if the parse succeeded, false otherwise.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// The produced value from the successful parse. If Success is false, this value is undefined.
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// The approximate location of the successful parse in the input sequence. On failure, this
        /// value is undefined and may show the location of the start of the attempt, the location at
        /// which failure occured, null, or some other value.
        /// </summary>
        Location Location { get; }

        /// <summary>
        /// Transforms the Value of the result to a new form
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        IParseResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform);
    }

    public static class ParseResultExtensions
    {
        /// <summary>
        /// Transforms the Value of the result to type object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IParseResult<object> Untype<T>(this IParseResult<T> result)
            => result.Transform(x => (object) x);
    }
}