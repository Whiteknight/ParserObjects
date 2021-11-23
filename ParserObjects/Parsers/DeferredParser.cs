﻿using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Looks up a parser at parse time, to avoid circular references in the grammar. The parser
/// looked up is expected to be constant for the duration of the parse and may be cached.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public abstract class Deferred<TInput, TOutput>
{
    public class Parser : IParser<TInput, TOutput>
    {
        private readonly Func<IParser<TInput, TOutput>> _getParser;

        public Parser(Func<IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var parser = _getParser() ?? throw new InvalidOperationException("Deferred parser value must not be null");
            return parser.Parse(state);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _getParser() };

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class MultiParser : IMultiParser<TInput, TOutput>
    {
        private readonly Func<IMultiParser<TInput, TOutput>> _getParser;

        public MultiParser(Func<IMultiParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var parser = _getParser() ?? throw new InvalidOperationException("Deferred parser value must not be null");
            return parser.Parse(state);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _getParser() };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
