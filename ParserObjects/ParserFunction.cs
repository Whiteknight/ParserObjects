namespace ParserObjects
{
    /// <summary>
    /// Create a failure result with the given error message.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="error"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public delegate IResult<TOutput> FailFunction<out TOutput>(string error, Location location = null);

    /// <summary>
    /// Create a success result with the given value.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="value"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public delegate IResult<TOutput> SuccessFunction<TOutput>(TOutput value, Location location = null);

    /// <summary>
    /// Execute a parse callback, with success and failure factory methods to create the necessary
    /// results.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="t"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <returns></returns>
    public delegate IResult<TOutput> ParserFunction<TInput, TOutput>(ParseState<TInput> t, SuccessFunction<TOutput> success, FailFunction<TOutput> fail);
}
