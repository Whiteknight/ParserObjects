using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Negative lookahead parser. Tests the input to see if the inner parser matches. Return success if the
    /// parser does not match, fail otherwise. Consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class NegativeLookaheadParser<TInput, TOutput> : IParser<TInput, bool>
    {
        private readonly IParser<TInput, TOutput> _inner;

        public NegativeLookaheadParser(IParser<TInput, TOutput> inner)
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
            return result.Success ? new FailResult<object>(result.Location) : (IParseResult<object>)new SuccessResult<object>(null, result.Location);
        }

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.Parse(window);
            window.Rewind();
            return result.Success ? new FailResult<bool>(result.Location) : (IParseResult<bool>)new SuccessResult<bool>(true, result.Location);
        }
    }
}