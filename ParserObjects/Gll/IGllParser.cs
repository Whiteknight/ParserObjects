namespace ParserObjects.Gll
{
    // TODO: IGllParser implement IParser
    public interface IGllParser : IParser
    {
        int Id { get; }
    }

    public interface IGllParser<TInput> : IGllParser
    {
        void Parse(IState<TInput> state, IResultPromise results);
    }

    // TOutput is just here as a marker, to help with type-inference
    public interface IGllParser<TInput, TOutput> : IGllParser<TInput>
    {
    }
}
