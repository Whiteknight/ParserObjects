namespace ParserObjects.Internal.Parsers;

public interface ISingleOrMultiParser<TInput, TOutput> : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
}
