using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    // Delegates to an internal parser, and also allows the internal parser to be
    // replaced without causing the entire parser tree to be rewritten.
    // Also if a child has been rewritten and the rewrite is bubbling up the tree, it will
    // stop here.
    public class ReplaceableParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private IParser<TInput, TOutput> _value;

        public ReplaceableParser(IParser<TInput, TOutput> defaultValue)
        {
            _value = defaultValue;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => _value.Parse(t);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_value == find && replace is IParser<TInput, TOutput> realReplace)
                _value = realReplace;
            return this;
        }

        public void SetParser(IParser<TInput, TOutput> parser)
        {
            _value = parser;
        }
    }
}