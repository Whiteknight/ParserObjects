using System;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput> Cache(IParser<TInput> parser)
        => new Function<TInput>.Parser<IParser<TInput>>(parser, static (p, state) =>
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult, SequenceCheckpoint>>(p, location);
            if (!cached.Success)
            {
                var result = p.Parse(state)!;
                state.Cache.Add(p, location, Tuple.Create(result, state.Input.Checkpoint()));
                return result;
            }

            var (cachedResult, continuation) = cached.Value;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();
            return cachedResult;
        }, "CACHED({child})", new[] { parser });

    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Cache<TOutput>(IParser<TInput, TOutput> parser)
        => new Function<TInput, TOutput>.Parser<IParser<TInput, TOutput>>(parser, static (p, args) =>
        {
            var location = args.Input.CurrentLocation;
            var cached = args.Cache.Get<Tuple<IResult<TOutput>, SequenceCheckpoint>>(p, location);
            if (!cached.Success)
            {
                var result = p.Parse(args.State);
                args.Cache.Add(p, location, Tuple.Create(result, args.Input.Checkpoint()));
                return result;
            }

            var (cachedResult, continuation) = cached.Value;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();

            return cachedResult;
        }, "CACHED({child})", new[] { parser });

    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Cache<TOutput>(IMultiParser<TInput, TOutput> parser)
        => new Function<TInput, TOutput>.MultiParser<IMultiParser<TInput, TOutput>>(parser, static (p, args) =>
        {
            var location = args.Input.CurrentLocation;
            var cached = args.Cache.Get<IMultiResult<TOutput>>(p, location);
            if (cached.Success)
                return cached.Value;

            var result = p.Parse(args.State);
            args.Cache.Add(p, location, result);
            return result;
        }, "CACHED({child})", new[] { parser });
}
