using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

/// <summary>
/// IParser extension methods for building combinators using fluent syntax.
/// </summary>
public static class ParserCombinatorExtensions
{
    /// <summary>
    /// Execute a parser and use the result to select the next parser to invoke. Executes a
    /// callback to find the next parser to invoke.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Func<IResult<TMiddle>, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => Parsers<TInput>.Chain(p, getNext, mentions);

    /// <summary>
    /// Execute a parser and use the result value to select the next parser to invoke. Uses
    /// a configuration object to setup parsers and the predicates which trigger them.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TInput, TOutput>(this IParser<TInput> p, Func<IResult, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => Parsers<TInput>.Chain(p, getNext, mentions);

    /// <summary>
    /// Execute a parser and use the result to select the nex parser to invoke. Uses a
    /// configuration object to store possible parsers and the input values which are used to
    /// select them.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Action<Chain<TInput, TMiddle, TOutput>.IConfiguration> setup)
        => Parsers<TInput>.ChainWith(p, setup);

    /// <summary>
    /// Execute a parser but consume no input. Use the result to select the next parser to
    /// invoke.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Choose<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Func<IResult<TMiddle>, IParser<TInput, TOutput>> getNext, params IParser[] mentions)
        => Parsers<TInput>.Chain(None(p), getNext, mentions);

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Examine<TInput, TOutput>(
        this IParser<TInput, TOutput> parser,
        Action<ParseContext<TInput, TOutput>>? before = null,
        Action<ParseContext<TInput, TOutput>>? after = null
    ) => Parsers<TInput>.Examine(parser, before, after);

    /// <summary>
    /// Invoke callbacks before and after a parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public static IParser<TInput> Examine<TInput>(
        this IParser<TInput> parser,
        Action<ParseContext<TInput>>? before = null,
        Action<ParseContext<TInput>>? after = null
    ) => Parsers<TInput>.Examine(parser, before, after);

    /// <summary>
    /// Zero-length assertion that the given parser's result is followed by another sequence.
    /// The lookahead sequence is matched but not consumed.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="lookahead"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> FollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
        => Parsers<TInput>.Combine(p, Parsers<TInput>.PositiveLookahead(lookahead)).Transform(r => (TOutput)r[0]);

    /// <summary>
    /// Returns a list of results from the given parser. Continues to parse until the parser returns
    /// failure. Returns an enumerable of results.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, bool atLeastOne)
        => Parsers<TInput>.List(p, atLeastOne);

    public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> separator, bool atLeastOne)
        => Parsers<TInput>.List(p, separator, atLeastOne);

    public static IParser<TInput> List<TInput>(this IParser<TInput> p, bool atLeastOne)
        => Parsers<TInput>.List(p, atLeastOne);

    public static IParser<TInput> List<TInput>(this IParser<TInput> p, IParser<TInput> separator, bool atLeastOne)
        => Parsers<TInput>.List(p, separator, atLeastOne);

    /// <summary>
    /// Returns a list of results from the given parser, with limits. Continues to
    /// parse until the parser returns failure or the maximum number of results is
    /// reached.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, int minimum, int? maximum = null)
        => Parsers<TInput>.List(p, Parsers<TInput>.Empty(), minimum, maximum);

    public static IParser<TInput> List<TInput>(this IParser<TInput> p, int minimum, int? maximum = null)
        => Parsers<TInput>.List(p, Parsers<TInput>.Empty(), minimum, maximum);

    /// <summary>
    /// Returns a list of results from the given parser, with limits. Continues to
    /// parse until the parser returns failure or the maximum number of results is
    /// reached.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput>? separator = null, int minimum = 0, int? maximum = null)
        => Parsers<TInput>.List(p, separator, minimum, maximum);

    public static IParser<TInput> List<TInput>(this IParser<TInput> p, IParser<TInput>? separator = null, int minimum = 0, int? maximum = null)
        => Parsers<TInput>.List(p, separator, minimum, maximum);

    /// <summary>
    /// Given a parser which parses characters, parse a list of characters and return the sequence as a
    /// string.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<char, string> ListCharToString(this IParser<char, char> p, bool atLeastOne)
        => ListCharToString(p, atLeastOne ? 1 : 0);

    /// <summary>
    /// Given a parser which parsers characters, parse a list of characters and return
    /// the result as a string. Supports limits for minimum and maximum numbers of
    /// characters to parse.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<char, string> ListCharToString(this IParser<char, char> p, int minimum = 0, int? maximum = null)
        => Transform(
            List(p, Parsers<char>.Empty(), minimum, maximum),
            CharMethods.ConvertToString
        );

    /// <summary>
    /// Given a parser which parses strings, parse a list of strings and return the sequence as a joined
    /// string.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<char, string> ListStringsToString(this IParser<char, string> p, bool atLeastOne = false)
        => Transform(
            List(p, atLeastOne),
            s => string.Concat(s)
        );

    /// <summary>
    /// Transform the output of the given parser. Synonym for Transform.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Map<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        => Parsers<TInput>.Transform(parser, transform);

    /// <summary>
    /// Wraps the given parser to guarantee that it consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> None<TInput, TOutput>(this IParser<TInput, TOutput> inner)
        => Parsers<TInput>.None(inner);

    /// <summary>
    /// Wraps the given parser to guarantee that it consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="inner"></param>
    /// <returns></returns>
    public static IParser<TInput> None<TInput>(this IParser<TInput> inner)
        => Parsers<TInput>.None(inner);

    /// <summary>
    /// Zero-length assertion that the given parser's match result is not followed by a lookahead pattern.
    /// The lookahead is compared but no input is consumed to match it.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="lookahead"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> NotFollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
        => Parsers<TInput>.Combine(p, Parsers<TInput>.NegativeLookahead(lookahead)).Transform(r => (TOutput)r[0]);

    /// <summary>
    /// The results of the given parser are optional. Returns success with an Option value
    /// which can be used to determine if the parser succeeded.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, Option<TOutput>> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p)
        => Parsers<TInput>.Optional(p);

    /// <summary>
    /// The results of the given parser are optional. If the inner parser fails, a default
    /// value is returned.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getDefault"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TOutput> getDefault)
        => Parsers<TInput>.Optional(p, getDefault);

    /// <summary>
    /// The results of the given parser are optional. If the given parser fails, a default value
    /// will be returned.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getDefault"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<IParseState<TInput>, TOutput> getDefault)
        => Parsers<TInput>.Optional(p, getDefault);

    /// <summary>
    /// Make this parser replaceable.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IParser<TInput, TOutput> p)
        => Parsers<TInput>.Replaceable(p);

    /// <summary>
    /// Make this parser replaceable.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput> Replaceable<TInput>(this IParser<TInput> p)
        => Parsers<TInput>.Replaceable(p);

    /// <summary>
    /// Make this parser replaceable.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IMultiParser<TInput, TOutput> p)
        => Parsers<TInput>.Replaceable(p);

    /// <summary>
    /// Make this parser replaceable. Gives the parser a name so that it can be easily
    /// found and replaced.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
        => Parsers<TInput>.Replaceable(p).Named(name);

    /// <summary>
    /// Make this parser replaceable. Gives the parser a name so that it can be easily found
    /// and replaced.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IMultiParser<TInput, TOutput> p, string name)
        => Parsers<TInput>.Replaceable(p).Named(name);

    /// <summary>
    /// Attempt the parse. Return on success. On failure, enter "panic mode" where input tokens can be
    /// discarded until the next "good" location and the parse will be attempted again. Subsequent
    /// attempts will always return failure, but with error information about all the errors which
    /// were seen.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="discardUntil"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Synchronize<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TInput, bool> discardUntil)
        => Parsers<TInput>.Synchronize<TOutput>(p, discardUntil);

    /// <summary>
    /// Transform the output of the given parser to a new value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        => Parsers<TInput>.Transform(parser, transform);
}
