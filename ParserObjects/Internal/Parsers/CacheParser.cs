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

    private readonly struct InternalParser<TParser, TResult>
        where TParser : IParser
    {
        private readonly Func<TParser, IParseState<TInput>, TResult> _parse;
        private readonly Func<TResult, bool> _isSuccess;

        public InternalParser(
            TParser parser,
            Func<TParser, IParseState<TInput>, TResult> parse,
            Func<TResult, bool> isSuccess
        )
        {
            Parser = parser;
            _parse = parse;
            _isSuccess = isSuccess;
        }

        public TParser Parser { get; }

        public TResult Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            var cached = state.Cache.Get<Tuple<TResult, SequenceCheckpoint>>(Parser, startCp.Location);
            if (!cached.Success)
            {
                var result = _parse(Parser, state);
                var cacheKey = Tuple.Create(result, startCp);
                state.Cache.Add(Parser, startCp.Location, cacheKey);
                return result;
            }

            var (cachedResult, continuation) = cached.Value;
            if (_isSuccess(cachedResult))
                continuation.Rewind();
            return cachedResult;
        }
    }

    public sealed record Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly InternalParser<IParser<TInput, TOutput>, Result<TOutput>> _internal;

        public Parser(IParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IParser<TInput, TOutput>, Result<TOutput>>(
                inner,
                static (p, s) => p.Parse(s),
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

        public IEnumerable<IParser> GetChildren() => [_internal.Parser];

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly InternalParser<IMultiParser<TInput, TOutput>, MultiResult<TOutput>> _internal;

        public MultiParser(IMultiParser<TInput, TOutput> inner, string name = "")
        {
            _internal = new InternalParser<IMultiParser<TInput, TOutput>, MultiResult<TOutput>>(
                inner,
                static (p, s) => p.Parse(s),
                static r => r.Success
            );
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public MultiResult<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        public INamed SetName(string name) => new MultiParser<TOutput>(_internal.Parser, name);

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public override string ToString() => DefaultStringifier.ToString(this);

        public IEnumerable<IParser> GetChildren() => [_internal.Parser];

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
