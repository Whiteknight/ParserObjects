using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

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

        private IResult<TCollection> _result;
        private TOutput[] _values;
        private int _index;

        public FlattenParser(IParser<TInput, TCollection> input)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            _input = input;
        }

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            if (_values != null)
                return GetNextResult(t);

            var result = _input.Parse(t);
            if (!result.Success)
                return result.Transform(_ => default(TOutput));

            var values = result.Value.ToArray();
            if (values.Length == 0)
                return t.Fail(this, "Inner parser returned an empy collection", result.Location);

            _result = result;
            _values = values;
            _index = 0;
            return GetNextResult(t);
        }

        private IResult<TOutput> GetNextResult(ParseState<TInput> t)
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

            return t.Success(this, value, location);
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

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
