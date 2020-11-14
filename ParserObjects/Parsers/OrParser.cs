using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Tests several parsers sequentially, returning Success if any parser succeeds, Failure
    /// otherwise. Consumes input but returns no explicit output.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class OrParser<TInput> : IParser<TInput, object>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;

        public OrParser(params IParser<TInput>[] parsers)
        {
            Assert.ArgumentNotNull(parsers, nameof(parsers));
            _parsers = parsers;
        }

        public string Name { get; set; }

        public IResult<object> Parse(ParseState<TInput> t) => ParseUntyped(t);

        public IResult<object> ParseUntyped(ParseState<TInput> t)
        {
            foreach (var parser in _parsers)
            {
                var result = parser.ParseUntyped(t);
                if (result.Success)
                    return result;
            }

            return t.Fail(this, "None of the given parser match");
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

            return new OrParser<TInput>(newList);
        }
    }
}