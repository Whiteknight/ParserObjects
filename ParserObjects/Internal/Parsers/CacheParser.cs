using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Cache the results of the inner parser for the current location. Every time this same parser is
/// attempted at the same location, the existing cached result will be returned immediately.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Cache<TInput>
{
    /* InternalParser is the shared implementation of Parse() and Match() methods. The various
     * Parser classes adapt this struct to the IParser interface variants.
     */

    private readonly struct InternalParser<TParser, TResult, TCacheEntry>
        where TParser : IParser
    {
        private readonly Func<TParser, IParseState<TInput>, TResult> _parse;
        private readonly Func<TResult, SequenceCheckpoint, TCacheEntry> _getCacheEntry;
        private readonly Func<TCacheEntry, (TResult, SequenceCheckpoint)> _decomposeCacheEntry;
        private readonly Func<TResult, bool> _isSuccess;

        public InternalParser(
            TParser parser,
            Func<TParser, IParseState<TInput>, TResult> parse,
            Func<TResult, SequenceCheckpoint, TCacheEntry> getCacheEntry,
            Func<TCacheEntry, (TResult, SequenceCheckpoint)> decomposeCacheEntry,
            Func<TResult, bool> isSuccess
        )
        {
            Parser = parser;
            _parse = parse;
            _getCacheEntry = getCacheEntry;
            _decomposeCacheEntry = decomposeCacheEntry;
            _isSuccess = isSuccess;
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
            if (!_isSuccess(cachedResult))
                return cachedResult;

            // Jump back to the position after the successful result.
            continuation.Rewind();
            return cachedResult;
        }
    }

    public sealed class Parser : IParser<TInput>
    {
        private readonly InternalParser<IParser<TInput>, Result<object>, Tuple<Result<object>, SequenceCheckpoint>> _internal;

        public Parser(IParser<TInput> inner, string name = "")
        {
            _internal = new InternalParser<IParser<TInput>, Result<object>, Tuple<Result<object>, SequenceCheckpoint>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, cp) => Tuple.Create(r, cp),
                static ce => (ce.Item1, ce.Item2),
                static r => r.Success
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public bool Match(IParseState<TInput> state) => _internal.Parse(state).Success;

        public Result<object> Parse(IParseState<TInput> state) => _internal.Parse(state).AsObject();

        public INamed SetName(string name) => new Parser(_internal.Parser, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly InternalParser<IParser<TInput, TOutput>, Result<TOutput>, Tuple<Result<TOutput>, SequenceCheckpoint>> _internal;

        public Parser(IParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IParser<TInput, TOutput>, Result<TOutput>, Tuple<Result<TOutput>, SequenceCheckpoint>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, cp) => Tuple.Create(r, cp),
                static ce => (ce.Item1, ce.Item2),
                static r => r.Success
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public bool Match(IParseState<TInput> state) => _internal.Parse(state).Success;

        public Result<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new Parser<TOutput>(_internal.Parser, name);

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public override string ToString() => DefaultStringifier.ToString(this);

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly InternalParser<IMultiParser<TInput, TOutput>, IMultResult<TOutput>, IMultResult<TOutput>> _internal;

        public MultiParser(IMultiParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IMultiParser<TInput, TOutput>, IMultResult<TOutput>, IMultResult<TOutput>>(
                inner,
                static (p, s) => p.Parse(s),
                static (r, _) => r,
                static ce => (ce, ce.StartCheckpoint),
                static r => r.Success
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IMultResult<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new MultiParser<TOutput>(_internal.Parser, name);

        IMultResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
