using System;
using System.Linq;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

public static partial class MultiParserExtensions
{
    /// <summary>
    /// Expect the IMultiResult to contain exactly 1 alternative, and select that to continue.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Single<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => multiParser.Select(args =>
        {
            if (args.Result.Results.Count == 1)
                return args.Success(args.Result.Results[0]);
            return args.Failure();
        });

    /// <summary>
    /// Select the result alternative which consumed the most amount of input and use that to
    /// continue the parse. If there are no alternatives, returns failure. If there are ties,
    /// the first is selected.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Longest<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => multiParser.Select(args =>
        {
            var longest = args.Result.Results
                .Where(r => r.Success)
                .OrderByDescending(r => r.Consumed)
                .FirstOrDefault();
            return longest != null ? args.Success(longest) : args.Failure();
        });

    /// <summary>
    /// Returns the first successful alternative which matches a predicate to continue the
    /// parse with.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser, Func<IResultAlternative<TOutput>, bool> predicate)
    {
        Assert.ArgumentNotNull(predicate, nameof(predicate));
        return multiParser.Select(args =>
        {
            var selected = args.Result.Results.FirstOrDefault(predicate);
            return selected != null ? args.Success(selected) : args.Failure();
        });
    }

    /// <summary>
    /// Selects the first successful alternative to continue the parse with.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => First(multiParser, r => r.Success);

    /// <summary>
    /// Invoke a special callback to attempt to select a single alternative and turn it into
    /// an IResult.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiparser"></param>
    /// <param name="select"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Select<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiparser, Func<Select<TInput, TOutput>.Arguments, IOption<IResultAlternative<TOutput>>> select)
        => new Select<TInput, TOutput>.Parser(multiparser, select);
}
