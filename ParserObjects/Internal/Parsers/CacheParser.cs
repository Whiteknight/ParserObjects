using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class Cache<TInput>
{
    private struct InternalParser<TParser, TResult, TCacheEntry>
        where TParser : IParser
        where TResult : IResultBase
    {
        private readonly Func<TParser, IParseState<TInput>, TResult> _parse;
        private readonly Func<TResult, SequenceCheckpoint, TCacheEntry> _getCacheEntry;
        private readonly Func<TCacheEntry, (TResult, SequenceCheckpoint)> _decomposeCacheEntry;

        public InternalParser(
            TParser parser,
            Func<TParser, IParseState<TInput>, TResult> parse,
            Func<TResult, SequenceCheckpoint, TCacheEntry> getCacheEntry,
            Func<TCacheEntry, (TResult, SequenceCheckpoint)> decomposeCacheEntry
        )
        {
            Parser = parser;
            _parse = parse;
            _getCacheEntry = getCacheEntry;
            _decomposeCacheEntry = decomposeCacheEntry;
        }

        public TParser Parser { get; }

        public TResult Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            var cached = state.Cache.Get<TCacheEntry>(Parser, startCp.Location);
            if (!cached.Success)
            {
                var result = _parse(Parser, state);
                var cacheKey = _getCacheEntry(result, startCp);
                state.Cache.Add(Parser, startCp.Location, cacheKey);
                return result;
            }

            var (cachedResult, continuation) = _decomposeCacheEntry(cached.Value);
            if (!cachedResult.Success)
                return cachedResult;

            continuation.Rewind();
            return cachedResult;
        }
    }

    public sealed class Parser : IParser<TInput>
    {
        private readonly InternalParser<IParser<TInput>, IResult, Tuple<IResult, SequenceCheckpoint>> _internal;

        public Parser(IParser<TInput> inner, string name = "")
        {
            _internal = new InternalParser<IParser<TInput>, IResult, Tuple<IResult, SequenceCheckpoint>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, cp) => Tuple.Create(r, cp),
                static ce => (ce.Item1, ce.Item2)
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public bool Match(IParseState<TInput> state) => _internal.Parse(state).Success;

        public IResult Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new Parser(_internal.Parser, name);

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public sealed record Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly InternalParser<IParser<TInput, TOutput>, IResult<TOutput>, Tuple<IResult<TOutput>, SequenceCheckpoint>> _internal;

        public Parser(IParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IParser<TInput, TOutput>, IResult<TOutput>, Tuple<IResult<TOutput>, SequenceCheckpoint>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, cp) => Tuple.Create(r, cp),
                static ce => (ce.Item1, ce.Item2)
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public bool Match(IParseState<TInput> state) => _internal.Parse(state).Success;

        public IResult<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new Parser<TOutput>(_internal.Parser, name);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly InternalParser<IMultiParser<TInput, TOutput>, IMultiResult<TOutput>, IMultiResult<TOutput>> _internal;

        public MultiParser(IMultiParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IMultiParser<TInput, TOutput>, IMultiResult<TOutput>, IMultiResult<TOutput>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, _) => r,
                static ce => (ce, ce.StartCheckpoint)
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new MultiParser<TOutput>(_internal.Parser, name);

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
