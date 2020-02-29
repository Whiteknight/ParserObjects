using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Negative lookahead parser. Tests the input to see if the inner parser matches. Return success if the
    /// parser does not match, fail otherwise. Consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NegativeLookaheadParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public NegativeLookaheadParser(IParser<TInput> inner)
        {
            _inner = inner;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput> typed)
                return new NegativeLookaheadParser<TInput>(typed);
            return this;
        }

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) 
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.ParseUntyped(window);
            window.Rewind();
            return result.Success ? new FailResult<object>(result.Location) : (IParseResult<object>)new SuccessResult<object>(null, result.Location);
        }
    }
}