namespace ParserObjects.Pratt
{
    public interface IParselet : INamed
    {
        int TokenTypeId { get; }
        int Lbp { get; }
        int Rbp { get; }
        IParser Parser { get; }
    }

    public interface IParselet<TInput, TOutput> : IParselet
    {
        (bool success, IToken<TInput, TOutput> token) TryGetNext(ParseState<TInput> state);
    }
}
