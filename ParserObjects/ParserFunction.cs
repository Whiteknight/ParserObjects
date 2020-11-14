namespace ParserObjects
{
    public delegate IResult<TOutput> FailFunction<out TOutput>(string error, Location location = null);
    public delegate IResult<TOutput> SuccessFunction<TOutput>(TOutput value, Location location = null);
    public delegate IResult<TOutput> ParserFunction<TInput, TOutput>(ParseState<TInput> t, SuccessFunction<TOutput> success, FailFunction<TOutput> fail);
}
