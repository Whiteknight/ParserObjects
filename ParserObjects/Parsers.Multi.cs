using System;
using System.Linq;
using ParserObjects.Internal;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Continue the parse with each alternative result separately.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWith<TMiddle, TOutput>(
        IMultiParser<TInput, TMiddle> p,
        GetParserFromParser<TInput, TMiddle, TOutput> getParser
    ) => new ContinueWith<TInput, TMiddle, TOutput>.SingleParser(p, getParser);

    /// <summary>
    /// Continue the parse with each alternative result separately.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWith<TMiddle, TOutput>(
        IMultiParser<TInput, TMiddle> p,
        GetMultiParserFromParser<TInput, TMiddle, TOutput> getParser
    ) => new ContinueWith<TInput, TMiddle, TOutput>.MultiParser(p, getParser);

    /// <summary>
    /// Continue the parse with each alternative result, followed by each parser
    /// returned by the callback.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getParsers"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWithEach<TMiddle, TOutput>(
        IMultiParser<TInput, TMiddle> p,
        GetParsersFromParser<TInput, TMiddle, TOutput> getParsers
    ) => ContinueWith(p,
        left => new EachParser<TInput, TOutput>(
            getParsers(left).ToArray(),
            string.Empty
        )
    );

    /// <summary>
    /// Continue the parse with only the first successful result alternative
    /// which matches the predicate.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> FirstResult<TOutput>(
        IMultiParser<TInput, TOutput> multiParser,
        Func<IResultAlternative<TOutput>, bool> predicate
    )
    {
        Assert.ArgumentNotNull(predicate);
        return SelectResult(multiParser, args =>
        {
            var selected = args.Result.Results.FirstOrDefault(predicate);
            return selected != null ? args.Success(selected) : args.Failure();
        });
    }

    /// <summary>
    /// Continue the parse with only the first successful result alternative.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> FirstResult<TOutput>(IMultiParser<TInput, TOutput> multiParser)
        => FirstResult(multiParser, static r => r.Success);

    /// <summary>
    /// Continue the parse with the longest successful result alternative.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> LongestResult<TOutput>(IMultiParser<TInput, TOutput> multiParser)
        => SelectResult(
            multiParser,
            static args =>
            {
                var longest = args.Result.Results
                    .Where(r => r.Success)
                    .OrderByDescending(r => r.Consumed)
                    .FirstOrDefault();
                return longest != null ? args.Success(longest) : args.Failure();
            }
        );

    /// <summary>
    /// Invoke a special callback to attempt to select a single alternative result
    /// and turn it into a single Result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="select"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> SelectResult<TOutput>(
        IMultiParser<TInput, TOutput> p,
        Func<SelectArguments<TOutput>, Option<IResultAlternative<TOutput>>> select
    ) => new SelectParser<TInput, TOutput>(p, select);

    /// <summary>
    /// Expect the parser to only return a single result alternative, and use
    /// that to continue the parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> SingleResult<TOutput>(IMultiParser<TInput, TOutput> multiParser)
        => SelectResult(multiParser, static args =>
        {
            if (args.Result.Results.Count == 1)
                return args.Success(args.Result.Results[0]);
            return args.Failure();
        });
}
