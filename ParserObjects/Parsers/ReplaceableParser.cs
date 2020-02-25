using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Delegates to an internal parser, and allows the internal parser to be replaced in-place without
    /// returning a new instance or causing a tree rewrite. 
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ReplaceableParser<TInput, TOutput> : IParser<TInput, TOutput>, IReplaceableParserUntyped
    {
        private IParser<TInput, TOutput> _value;

        public ReplaceableParser(IParser<TInput, TOutput> defaultValue)
        {
            _value = defaultValue;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => _value.Parse(t);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => _value.ParseUntyped(t);

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_value == find && replace is IParser<TInput, TOutput> realReplace)
                _value = realReplace;
            return this;
        }

        public IParser ReplaceableChild => _value;

        public void SetParser(IParser parser)
        {
            if (parser is IParser<TInput, TOutput> typed)
                _value = typed;
        }
    }
}