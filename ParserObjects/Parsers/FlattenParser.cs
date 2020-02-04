using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Flattens an enumerable result to a series of individual result items
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FlattenParser<TInput, TCollection, TOutput> : IParser<TInput, TOutput>
        where TCollection : IEnumerable<TOutput>
    {
        private readonly IParser<TInput, TCollection> _input;

        private IParseResult<TCollection> _result;
        private TOutput[] _values;
        private int _index;

        public FlattenParser(IParser<TInput, TCollection> input)
        {
            _input = input;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            if (_values != null)
                return GetNextResult();

            var result = _input.Parse(t);
            if (!result.Success)
                return new FailResult<TOutput>(result.Location);
            var values = result.Value.ToArray();
            if (values.Length == 0)
                return new FailResult<TOutput>(result.Location);

            _result = result;
            _values = values;
            _index = 0;
            return GetNextResult();
        }

        private IParseResult<TOutput> GetNextResult()
        {
            var location = _result.Location;
            var value = _values[_index];
            _index++;
            if (_index >= _values.Length)
            {
                _result = null;
                _values = null;
                _index = -1;
            }

            return new SuccessResult<TOutput>(value, location);
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _input };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_input == find && replace is IParser<TInput, TCollection> typed)
                return new FlattenParser<TInput, TCollection, TOutput>(typed);
            return this;
        }
    }
}
