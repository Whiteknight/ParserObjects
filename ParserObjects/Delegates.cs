using System.Collections.Generic;

namespace ParserObjects;

/// <summary>
/// Create a new IResultAlternative starting from an existing result.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="existing"></param>
/// <param name="factory"></param>
/// <returns></returns>
public delegate IResultAlternative<TOutput> CreateNewResultAlternative<TOutput>(IResultAlternative<TOutput> existing, ResultAlternativeFactoryMethod<TOutput> factory);

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
/// <typeparam name="TOutput"></typeparam>
/// <param name="result"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> GetParserFromResult<TInput, TOutput>(IResult result);

/// <summary>
/// Create a new parser, given an existing result.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="result"></param>
/// <returns></returns>
public delegate IParser<TInput, TOutput> GetParserFromResult<TInput, TMiddle, TOutput>(IResult<TMiddle> result);

/// <summary>
/// Create an enumerable of parsers, given an existing parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="p"></param>
/// <returns></returns>
public delegate IEnumerable<IParser<TInput, TOutput>> GetParsersFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

/// <summary>
/// Factory method for creating a new result alternative of the same type.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="value"></param>
/// <param name="consumed"></param>
/// <param name="continuation"></param>
/// <returns></returns>
public delegate IResultAlternative<TOutput> ResultAlternativeFactoryMethod<TOutput>(TOutput value, int consumed, SequenceCheckpoint continuation);
