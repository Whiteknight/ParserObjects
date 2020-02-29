using System.Collections.Generic;
using System.Linq;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers.Logical
{
    public class AndParser<TInput> : IParser<TInput, object>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;

        public AndParser(params IParser<TInput>[] parsers)
        {
            _parsers = parsers;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (!_parsers.Contains(find) || !(replace is IParser<TInput> realReplace))
                return this;
            var newList = new IParser<TInput>[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var child = _parsers[i];
                newList[i] = child == find ? realReplace : child;
            }

            return new AndParser<TInput>(newList);
        }

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            foreach (var parser in _parsers)
            {
                var result = parser.ParseUntyped(t);
                if (!result.Success)
                    return new FailResult<object>(t.CurrentLocation);
            }

            return new SuccessResult<object>(null, t.CurrentLocation);
        }
    }
}
