using System.Collections.Generic;

namespace ParserObjects.Parsers
{
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