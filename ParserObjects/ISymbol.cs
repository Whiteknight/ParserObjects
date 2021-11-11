namespace ParserObjects
{
    // marker interface for items which may act as a symbol in a grammar and also the top-most
    // interface type for all parsers and similar objects
    public interface ISymbol : INamed
    {
    }

    // marker interface for items which may act as a symbol in a grammar with a specific output
    // type.
    public interface ISymbol<out TValue> : ISymbol
    {
    }
}
