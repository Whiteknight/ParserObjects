using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Looks up a parser at parse time, to avoid circular references in the grammar. The parser
    /// looked up is expected to be constant for the duration of the parse and may be cached.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class DeferredParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IParser<TInput, TOutput>> _getParser;

        public DeferredParser(Func<IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            var parser = _getParser();
            if (parser == null)
                throw new InvalidOperationException("Deferred parser value must not be null");
            return parser.Parse(state);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _getParser() };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
