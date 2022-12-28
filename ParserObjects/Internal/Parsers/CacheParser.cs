using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class Cache<TInput>
{
    public sealed record Parser(
        IParser<TInput> Inner,
        string Name = ""
    ) : IParser<TInput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult, SequenceCheckpoint>>(Inner, location);
            if (cached.Success)
            {
                var (cachedResult, continuation) = cached.Value;
                continuation.Rewind();
                return cachedResult.Success;
            }

            var result = Inner.Parse(state);
            state.Cache.Add(Inner, location, Tuple.Create(result, state.Input.Checkpoint()));
            return result.Success;
        }

        public IResult Parse(IParseState<TInput> state)
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult, SequenceCheckpoint>>(Inner, location);
            if (!cached.Success)
            {
                var result = Inner.Parse(state)!;
                state.Cache.Add(Inner, location, Tuple.Create(result, state.Input.Checkpoint()));
                return result;
            }

            var (cachedResult, continuation) = cached.Value;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();
            return cachedResult;
        }

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public sealed record Parser<TOutput>(
        IParser<TInput, TOutput> Inner,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult, SequenceCheckpoint>>(Inner, location);
            if (cached.Success)
            {
                var (cachedResult, continuation) = cached.Value;
                continuation.Rewind();
                return cachedResult.Success;
            }

            var result = Inner.Parse(state);
            state.Cache.Add(Inner, location, Tuple.Create(result, state.Input.Checkpoint()));
            return result.Success;
        }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<Tuple<IResult<TOutput>, SequenceCheckpoint>>(Inner, location);
            if (!cached.Success)
            {
                var result = Inner.Parse(state);
                state.Cache.Add(Inner, location, Tuple.Create(result, state.Input.Checkpoint()));
                return result;
            }

            var (cachedResult, continuation) = cached.Value;
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();

            return cachedResult;
        }

        public INamed SetName(string name) => this with { Name = name };

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public sealed record MultiParser<TOutput>(
        IMultiParser<TInput, TOutput> Inner,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var location = state.Input.CurrentLocation;
            var cached = state.Cache.Get<IMultiResult<TOutput>>(Inner, location);
            if (cached.Success)
                return cached.Value;

            var result = Inner.Parse(state);
            state.Cache.Add(Inner, location, result);
            return result;
        }

        public INamed SetName(string name) => this with { Name = name };

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
