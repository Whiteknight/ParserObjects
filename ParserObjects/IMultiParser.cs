namespace ParserObjects
{
    public interface IMultiParser<TInput> : IParser
    {
        IMultiResult Parse(IParseState<TInput> state);
    }

    public interface IMultiParser<TInput, TOutput> : IMultiParser<TInput>
    {
        // MultiParser should return the input to the starting location, because every result
        // has a .Continuation checkpoint which can be invoked, and an error result might be used
        // for continuation.
        new IMultiResult<TOutput> Parse(IParseState<TInput> state);
    }
}
