namespace ParserObjects
{
    public delegate Result<TOutput> FailFunction<TOutput>(string error, Location location = null);
    public delegate Result<TOutput> SuccessFunction<TOutput>(TOutput value, Location location = null);
    public delegate Result<TOutput> ParserFunction<TInput, TOutput>(ParseState<TInput> t, SuccessFunction<TOutput> success, FailFunction<TOutput> fail);
}
