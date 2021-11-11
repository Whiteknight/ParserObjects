namespace ParserObjects
{
    public interface IMultiParser<in TInput> : IParser
    {
        IMultiResult Parse(IParseState<TInput> state);
    }

    public interface IMultiParser<in TInput, TOutput> : IMultiParser<TInput>, ISymbol<TOutput>
    {
        // MultiParser should return the input to the starting location, because every result
        // has a .Continuation checkpoint which can be invoked, and an error result might be used
        // for continuation.
        new IMultiResult<TOutput> Parse(IParseState<TInput> state);
    }
}
