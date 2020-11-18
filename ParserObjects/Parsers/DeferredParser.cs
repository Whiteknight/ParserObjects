using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Looks up a parser at Parse() time, to avoid circular references in the grammar
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class DeferredParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IParser<TInput, TOutput>> _getParser;

        public DeferredParser(Func<IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            var parser = _getParser();
            if (parser == null)
                throw new InvalidOperationException("Deferred parser value must not be null");
            return parser.Parse(t);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _getParser() };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _getParser() && replace is IParser<TInput, TOutput> realReplace)
                return realReplace;
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
