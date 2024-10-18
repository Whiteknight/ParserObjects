using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects;

/// <summary>
/// Parser base type.
/// </summary>
public interface IParser : ISymbol
{
    /// <summary>
    /// Gets a unique ID for this parser instance.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Get a list of child parsers, if any.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IParser> GetChildren();

    void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>;
}

/// <summary>
/// Parser object which allows getting the result without type information.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IParser<in TInput> : IParser
{
    /// <summary>
    /// Attempt to parse the input sequence and produce an output result of type object. If the
    /// parse fails, it is expected that this method will return the input sequence to the
    /// state it was at before the parse was attempted.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    IResult Parse(IParseState<TInput> state);

    /// <summary>
    /// Attempt to match the input sequence and return a boolean. If the match fails, it is
    /// expected that this method will return the input sequence to the state it was at before the
    /// match was attempted.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    bool Match(IParseState<TInput> state);
}

/// <summary>
/// Parser with explicit input and output types.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IParser<in TInput, out TOutput> : IParser<TInput>, ISymbol<TOutput>
{
    /// <summary>
    /// Attempt to parse the input sequence and produce an output result. If the parse fails,
    /// it is expected that this method will return the input sequence to the state it was at
    /// before the parse was attempted.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    new IResult<TOutput> Parse(IParseState<TInput> state);
}

/// <summary>
/// A parser which has an in-place replaceable child. Used to identify parsers which can
/// participate in certain find/replace operations.
/// </summary>
public interface IReplaceableParserUntyped : IParser
{
    /// <summary>
    /// Gets the child parser which can be replaced without cloning.
    /// </summary>
    IParser ReplaceableChild { get; }

    /// <summary>
    /// Set the new child parser without cloning.
    /// </summary>
    /// <param name="parser"></param>
    SingleReplaceResult SetParser(IParser parser);
}
