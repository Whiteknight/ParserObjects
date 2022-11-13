using System;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class ParserMethods<TInput>
{
    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput> Cache(IParser<TInput> p)
        => new Function<TInput>.Parser(state =>
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult, ISequenceCheckpoint>>(p, location);
            if (!cached.Success)
            {
                var result = p.Parse(state)!;
                state.Cache.Add(p, location, Tuple.Create(result, state.Input.Checkpoint()));
                return result;
            }

            var entry = cached.Value;
            var (cachedResult, continuation) = entry;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();
            return cachedResult;
        }, "CACHED({child})", new[] { p });

    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Cache<TOutput>(IParser<TInput, TOutput> p)
        => new Function<TInput, TOutput>.Parser(args =>
        {
            var location = args.Input.CurrentLocation;
            var cached = args.Cache.Get<Tuple<IResult<TOutput>, ISequenceCheckpoint>>(p, location);
            if (!cached.Success)
            {
                var result = p.Parse(args.State);
                args.Cache.Add(p, location, Tuple.Create(result, args.Input.Checkpoint()));
                return result;
            }

            var entry = cached.Value;
            var (cachedResult, continuation) = entry;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();

            return cachedResult;
        }, "CACHED({child})", new[] { p });

    /// <summary>
    /// Cache the eresult of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Cache<TOutput>(IMultiParser<TInput, TOutput> p)
        => new Function<TInput, TOutput>.MultiParser(args =>
        {
            var location = args.Input.CurrentLocation;
            var cached = args.Cache.Get<IMultiResult<TOutput>>(p, location);
            if (cached.Success)
                return cached.Value;

            var result = p.Parse(args.State);
            args.Cache.Add(p, location, result);
            return result;
        }, "CACHED({child})", new[] { p });
}
