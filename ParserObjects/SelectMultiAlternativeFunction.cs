namespace ParserObjects
{
    public delegate IOption<IMultiResultAlternative<TOutput>> SuccessMultiAlternativeFunction<TOutput>(IMultiResultAlternative<TOutput> alt);

    public delegate IOption<IMultiResultAlternative<TOutput>> FailureMultiAlternativeFunction<TOutput>();

    public delegate IOption<IMultiResultAlternative<TOutput>> SelectMultiAlternativeFunction<TOutput>(IMultiResult<TOutput> result, SuccessMultiAlternativeFunction<TOutput> success, FailureMultiAlternativeFunction<TOutput> fail);
}
