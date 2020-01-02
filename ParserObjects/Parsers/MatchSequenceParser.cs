using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class MatchSequenceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly TInput[] _find;
        private readonly Func<TInput[], TOutput> _produce;

        public MatchSequenceParser(IEnumerable<TInput> find, Func<TInput[], TOutput> produce)
        {
            _find = find.ToArray();
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;

            // Handle a few small cases where we don't want to allocate a buffer
            if (t.IsAtEnd && _find.Length > 0)
                return new FailResult<TOutput>(location);
            if (_find.Length == 0)
                return new SuccessResult<TOutput>(_produce(new TInput[0]), location);
            if (_find.Length == 1)
                return t.Peek().Equals(_find[0]) ? new SuccessResult<TOutput>(_produce(new[] { t.GetNext() }), location) : (IParseResult<TOutput>)new FailResult<TOutput>(location);

            var buffer = new TInput[_find.Length];
            for (var i = 0; i < _find.Length; i++)
            {
                var c = t.GetNext();
                buffer[i] = c;
                if (c.Equals(_find[i]))
                    continue;

                for (; i >= 0; i--)
                    t.PutBack(buffer[i]);
                return new FailResult<TOutput>();
            }

            return new SuccessResult<TOutput>(_produce(buffer), location);
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}