using ParserObjects.Internal;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Tests several parsers sequentially. Returns success if they all succeed, otherwise
    /// returns failure. Consumes input but returns no explicit output.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, object> And(params IParser<TInput>[] parsers)
        => new CaptureParser<TInput, object>(parsers, static (_, _, _) => Defaults.ObjectInstance);

    /// <summary>
    /// Tests the predicate parser. If the predicate succeeds, invoke the then parser
    /// Otherwise return Failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="thenParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> If<TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> thenParser)
        => If(predicate, thenParser, Fail<TOutput>());

    /// <summary>
    /// Tests the predicate parser. If the predicate succeeds, invoke the then parser.
    /// Otherwise invokes the else parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="thenParser"></param>
    /// <param name="elseParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> If<TOutput>(
        IParser<TInput> predicate,
        IParser<TInput, TOutput> thenParser,
        IParser<TInput, TOutput> elseParser
    ) => new IfParser<TInput, TOutput>(predicate, thenParser, elseParser);

    /// <summary>
    /// Invoke the given parser and invert the result. On Success return Failure, on Failure return
    /// Success. Consumes no input and produces no output value.
    /// </summary>
    /// <param name="p1"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Not(IParser<TInput> p1)
        => new NegativeLookaheadParser<TInput>(p1);

    /// <summary>
    /// Tests several parsers sequentially. Returns Success if any parser succeeds, returns
    /// Failure otherwise. Consumes input but returns no explicit output. Synonym for First().
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, object> Or(params IParser<TInput>[] parsers)
        => First(parsers);
}
