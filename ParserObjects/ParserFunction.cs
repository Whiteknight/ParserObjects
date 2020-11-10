namespace ParserObjects
{
    public delegate IResult<TOutput> FailFunction<TOutput>(Location location = null);
    public delegate IResult<TOutput> SuccessFunction<TOutput>(TOutput value, Location location = null);
    public delegate IResult<TOutput> ParserFunction<TInput, TOutput>(ISequence<TInput> t, SuccessFunction<TOutput> success, FailFunction<TOutput> fail);
}
