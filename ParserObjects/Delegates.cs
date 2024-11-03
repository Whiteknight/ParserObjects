using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// Create a new ResultAlternative starting from an existing result.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="existing"></param>
/// <param name="factory"></param>
/// <returns></returns>
public delegate ResultAlternative<TOutput> CreateNewResultAlternative<TOutput>(ResultAlternative<TOutput> existing, ResultAlternativeFactoryMethod<TOutput> factory);

/// <summary>
/// Get or create a parser given the current parse state.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="state"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> CreateParserFromState<TInput, TOutput>(IParseState<TInput> state);

/// <summary>
/// Get or create a parser given the current parse state.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="state"></param>
/// <returns></returns>
public delegate IMultiParser<TInput, TOutput> CreateMultiParserFromState<TInput, TOutput>(IParseState<TInput> state);

/// <summary>
/// Create a new parser, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IMultiParser<TInput, TOutput> GetMultiParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

/// <summary>
/// Create a new parser, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IMultiParser<TInput, TOutput> GetMultiParserFromMultiParser<TInput, TOutput>(IMultiParser<TInput, TOutput> p);

/// <summary>
/// Create a new parser, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TOutput>(IParser<TInput, TOutput> p);

/// <summary>
/// Create a new parser, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

/// <summary>
/// Create a new parser, given an existing result.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="result"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> GetParserFromResult<TInput, TMiddle, TOutput>(Result<TMiddle> result);

/// <summary>
/// Create an enumerable of parsers, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IEnumerable<IParser<TInput, TOutput>> GetParsersFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

public delegate ResultAlternative<TOutput> SelectResultFromMultiResult<TOutput>(MultiResult<TOutput> result);

/// <summary>
/// Factory method for creating a new result alternative of the same type.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="value"></param>
/// <param name="consumed"></param>
/// <param name="continuation"></param>
/// <returns></returns>
public delegate ResultAlternative<TOutput> ResultAlternativeFactoryMethod<TOutput>(TOutput value, int consumed, SequenceCheckpoint continuation);
