namespace ParserObjects.Pratt
{
    public delegate TOutput NudFunc<TInput, TValue, TOutput>(IParseContext<TInput, TOutput> context, IToken<TValue> value);

    public delegate TOutput LedFunc<TInput, TValue, TOutput>(IParseContext<TInput, TOutput> context, IToken<TOutput> left, IToken<TValue> value);
}