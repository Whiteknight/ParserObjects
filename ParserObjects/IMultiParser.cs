namespace ParserObjects
{
    // TODO: Make sure this inheriting from IParser doesn't imply it can be mixed in with other
    // combinators. May need a new interface or some guards or something. Don't want this getting
    // dumped into RuleParser or anything like that
    public interface IMultiParser<TInput, TOutput> : IParser
    {
        // MultiParser should return the input to the starting location, because every result
        // has a .Continuation checkpoint which can be invoked, and an error result might be used
        // for continuation.
        IMultiResult<TOutput> Parse(IParseState<TInput> state);
    }
}
