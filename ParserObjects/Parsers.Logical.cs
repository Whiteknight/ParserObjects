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
    public static IParser<TInput> And(params IParser<TInput>[] parsers)
        => new AndParser<TInput>(parsers);

    /// <summary>
    /// Tests the predicate parser. If the predicate succeeds, invoke the success parser
    /// Otherwise return Failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="onSuccess"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> If<TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> onSuccess)
        => new IfParser<TInput, TOutput>(predicate, onSuccess, Fail<TOutput>());

    /// <summary>
    /// Tests the predicate parser. If the predicate succeeds, invoke the success parser.
    /// Otherwise invokes the failure parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onFail"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> If<TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> onSuccess, IParser<TInput, TOutput> onFail)
        => new IfParser<TInput, TOutput>(predicate, onSuccess, onFail);

    /// <summary>
    /// Invoke the given parser and invert the result. On Success return Failure, on Failure return
    /// Success. Consumes input but returns no output.
    /// </summary>
    /// <param name="p1"></param>
    /// <returns></returns>
    public static IParser<TInput> Not(IParser<TInput> p1)
        => new NotParser<TInput>(p1);

    /// <summary>
    /// Tests several parsers sequentially. Returns Success if any parser succeeds, returns
    /// Failure otherwise. Consumes input but returns no explicit output.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput> Or(params IParser<TInput>[] parsers)
        => new OrParser<TInput>(parsers);
}
