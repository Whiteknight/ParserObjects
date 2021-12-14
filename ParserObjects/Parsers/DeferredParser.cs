using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Looks up a parser at parse time, to avoid circular references in the grammar. The parser
/// looked up is expected to be constant for the duration of the parse and may be cached.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Deferred<TInput, TOutput>
{
    public sealed record Parser(
        Func<IParser<TInput, TOutput>> GetParser,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var parser = GetParserFromCacheOrCallback(state);
            return parser.Parse(state);
        }

        private IParser<TInput, TOutput> GetParserFromCacheOrCallback(IParseState<TInput> state)
        {
            var existing = state.Cache.Get<IParser<TInput, TOutput>>(this, default);
            if (existing.Success)
                return existing.Value;

            var parser = GetParser() ?? throw new InvalidOperationException("Deferred parser value must not be null");
            state.Cache.Add(this, default, parser);
            return parser;
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { GetParser() };

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => this with { Name = name };
    }

    public sealed record MultiParser(
        Func<IMultiParser<TInput, TOutput>> GetParser,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var parser = GetParserFromCacheOrCallback(state);
            return parser.Parse(state);
        }

        private IMultiParser<TInput, TOutput> GetParserFromCacheOrCallback(IParseState<TInput> state)
        {
            var existing = state.Cache.Get<IMultiParser<TInput, TOutput>>(this, default);
            if (existing.Success)
                return existing.Value;

            var parser = GetParser() ?? throw new InvalidOperationException("Deferred parser value must not be null");
            state.Cache.Add(this, default, parser);
            return parser;
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { GetParser() };

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => this with { Name = name };
    }
}
