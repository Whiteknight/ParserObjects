using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Delegates to an internal parser, and allows the internal parser to be replaced without causing a
    /// tree-rewrite. If a child has been rewritten and the rewrite is bubbline up the tree, it will stop
    /// here. Useful only if your grammar allows rule rewrites.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
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