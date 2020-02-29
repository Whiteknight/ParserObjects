using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Does a lookahead to see if there is a match. Returns a success or failure result, but does not
    /// consume any actual input
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class PositiveLookaheadParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public PositiveLookaheadParser(IParser<TInput> inner)
        {
            _inner = inner;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput> typed)
                return new PositiveLookaheadParser<TInput>(typed);
            return this;
        }

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) 
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.ParseUntyped(window);
            window.Rewind();
            return result.Transform(v => (object) null);
        }
    }
}