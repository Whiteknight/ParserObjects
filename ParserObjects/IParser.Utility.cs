using System;
using System.Collections.Generic;
using ParserObjects.Internal.Bnf;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Sequences;

namespace ParserObjects;

public static class ParserExtensions
{
    public static IReadOnlyDictionary<int, IParser> GetAllParsers(this IParser p)
        => new ListParsersVisitor().Visit(p);

    /// <summary>
    /// Attempt to describe the parser as a string of pseudo-BNF. This feature depends on parsers having
    /// a .Name value set. If you are using custom IParser implementations you will need to use a custom
    /// BnfStringifyVisitor subclass to account for it.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static string ToBnf(this IParser parser)
    {
        if (string.IsNullOrEmpty(parser.Name))
            parser = parser.Named("(TARGET)");

        return BnfStringifier.Instance.Stringify(parser);
    }

    /// <summary>
    /// Convert a parser and it's input sequence into a new sequence of parse result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="input"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static ISequence<IResult<TOutput>> ToSequence<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input, Action<string>? log = null)
        => FromParseResult(input, parser, log);
}
