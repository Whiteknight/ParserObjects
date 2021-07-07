namespace ParserObjects
{
    // marker interface for items which may act as a symbol in a grammar
    public interface ISymbol : INamed
    {
    }

    // marker interface for items which may act as a symbol in a grammar with a specific output
    // type.
    public interface ISymbol<out TValue> : ISymbol
    {
    }
}
