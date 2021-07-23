namespace ParserObjects
{
    public delegate IOption<IResultAlternative<TOutput>> SuccessMultiAlternativeFunction<TOutput>(IResultAlternative<TOutput> alt);

    public delegate IOption<IResultAlternative<TOutput>> FailureMultiAlternativeFunction<TOutput>();

    public delegate IOption<IResultAlternative<TOutput>> SelectMultiAlternativeFunction<TOutput>(IMultiResult<TOutput> result, SuccessMultiAlternativeFunction<TOutput> success, FailureMultiAlternativeFunction<TOutput> fail);
}
