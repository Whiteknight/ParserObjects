using System;
using System.Collections.Generic;
using ParserObjects.Internal.Bnf;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Sequences;

namespace ParserObjects;

public static class ParserExtensions
{
    /// <summary>
    /// Get a list of all referenced parsers from the given parser and it's children.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<int, IParser> GetAllParsers(this IParser p)
        => ListParsersVisitor.Visit(p);

    /// <summary>
    /// Attempt to describe the parser as a string of pseudo-BNF. This feature depends on parsers having
    /// a .Name value set. If you are using custom IParser implementations you will need to use a custom
    /// BnfStringifyVisitor subclass to account for it.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static string ToBnf(this IParser parser)
        => BnfStringifier.Instance.Stringify(
            string.IsNullOrEmpty(parser.Name)
                ? parser.Named("(TARGET)")
                : parser
        );

    /// <summary>
    /// Convert a parser and it's input sequence into a new sequence of parse result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="getEndSentinel"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static ISequence<Result<TOutput>> ToSequence<TInput, TOutput>(
        this IParser<TInput, TOutput> parser,
        ISequence<TInput> input,
        Func<ResultFactory<TInput, TOutput>, Result<TOutput>>? getEndSentinel = null,
        Action<string>? log = null
    ) => FromParseResult(input, parser, getEndSentinel, log);
}
