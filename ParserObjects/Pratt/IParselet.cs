namespace ParserObjects.Pratt
{
    public interface IParselet<TInput, TOutput> : INamed
    {
        int TokenTypeId { get; }
        int Lbp { get; }
        int Rbp { get; }
        IParser Parser { get; }

        (bool success, IToken<TInput, TOutput> token) TryGetNext(ParseState<TInput> state);

        public bool CanNud { get; }
        public bool CanLed { get; }
    }
}
