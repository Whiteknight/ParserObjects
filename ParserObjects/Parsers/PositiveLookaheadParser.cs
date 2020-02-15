using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Does a lookahead to see if there is a match. Returns a success or failure result, but does not
    /// consume any actual input
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class PositiveLookaheadParser<TInput, TOutput> : IParser<TInput, bool>
    {
        private readonly IParser<TInput, TOutput> _inner;

        public PositiveLookaheadParser(IParser<TInput, TOutput> inner)
        {
            _inner = inner;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput, TOutput> typed)
                return new PositiveLookaheadParser<TInput, TOutput>(typed);
            return this;
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.Parse(window);
            window.Rewind();
            return result.Transform(c => (object) result.Success);
        }

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.Parse(window);
            window.Rewind();
            return result.Transform(c => result.Success);
        }
    }
}