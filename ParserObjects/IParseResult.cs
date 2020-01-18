using System;

namespace ParserObjects
{
    /// <summary>
    /// Result object from a Parse operation
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParseResult<out TOutput>
    {
        /// <summary>
        /// Returns true if the parse succeeded, false otherwise.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// The produced value from the successful parse. If Success is false, this value is undefined.
        /// </summary>
        TOutput Value { get; }

        /// <summary>
        /// The approximate location of the successful parse in the input sequence. On failure, this
        /// value is undefined and may show the location of the start of the attempt, the location at
        /// which failure occured, null, or some other value.
        /// </summary>
        Location Location { get; }

        /// <summary>
        /// Return a new IParseResult without explicit type information.
        /// </summary>
        /// <returns></returns>
        IParseResult<object> Untype();
    }

    public static class ParseResultExtensions
    {
        /// <summary>
        /// If the result is a success, transform the value and return a new result object with the updated
        /// value. If the result is a failure, returns a new failure result of the new type.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParseResult<TOutput> Transform<TInput, TOutput>(this IParseResult<TInput> input, Func<TInput, TOutput> transform)
        {
            if (input.Success)
                return new SuccessResult<TOutput>(transform(input.Value), input.Location);
            return new FailResult<TOutput>(input.Location);
        }
    }
}