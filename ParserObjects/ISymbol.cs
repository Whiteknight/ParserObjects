namespace ParserObjects;

#pragma warning disable S2326

/// <summary>
/// Marker interface for types which can be used as grammar symbols.
/// </summary>
public interface ISymbol : INamed
{
}

/// <summary>
/// Marker interface for types which can be used as grammar symbols with typed output.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface ISymbol<out TValue> : ISymbol
{
}
