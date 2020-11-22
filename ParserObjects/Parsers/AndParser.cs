using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Tests several parsers sequentially. If all of them succeed return Success. If any Fail, 
    /// return Failure. Consumes input but returns no explicit output.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class AndParser<TInput> : IParser<TInput>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;

        public AndParser(params IParser<TInput>[] parsers)
        {
            Assert.ArgumentNotNull(parsers, nameof(parsers));
            _parsers = parsers;
        }

        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startLocation = state.Input.CurrentLocation;
            var checkpoint = state.Input.Checkpoint();
            foreach (var parser in _parsers)
            {
                var result = parser.Parse(state);
                if (!result.Success)
                {
                    checkpoint.Rewind();
                    return result;
                }
            }

            return state.Success(this, null, startLocation);
        }

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == null || replace == null)
                return this;
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
    }
}
