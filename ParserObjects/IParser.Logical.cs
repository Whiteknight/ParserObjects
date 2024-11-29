namespace ParserObjects;

public static class ParserLogicalExtensions
{
    /// <summary>
    /// Parse the given parser and all additional parsers sequentially. Consumes input but returns no
    /// output. Will probably be used by Positive- or Negative-lookahead or If.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, object> And<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
        => Parsers<TInput>.And([p1, .. parsers]);

    /// <summary>
    /// Attempt to parse with a predicate parser, consuming no input. If the predicate parser succeeds,
    /// parse with the given parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> If<TInput, TOutput>(
        this IParser<TInput, TOutput> parser,
        IParser<TInput> predicate
    ) => Parsers<TInput>.If(predicate, parser, Parsers<TInput>.Fail<TOutput>());

    /// <summary>
    /// Parses with the given parser, inverting the result so Success becomes Failure and Failure becomes
    /// Success. Consumes input but returns no output. Will probably be used by Positive- or
    /// Negative-lookahead or If.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="p1"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Not<TInput>(this IParser<TInput> p1)
        => Parsers<TInput>.Not(p1);

    /// <summary>
    /// Attempts to parse with each parser successively, returning Success if any parser succeeds
    /// or Failure if none do. Consumes input but returns no output. Will probably be used by
    /// Positive- or Negative-lookahed or If.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Or<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
        => Parsers<TInput>.Or([p1, .. parsers]);

    /// <summary>
    /// Attempt to parse with a predicate parser. If the predicate parser succeeds,
    /// parse with the given parser. This is the same operation as If with different order of operands.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Then<TInput, TOutput>(this IParser<TInput> predicate, IParser<TInput, TOutput> parser)
        => Parsers<TInput>.If(predicate, parser, Parsers<TInput>.Fail<TOutput>());
}
