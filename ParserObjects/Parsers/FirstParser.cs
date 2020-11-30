using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
    /// succeeds
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class FirstParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput, TOutput>> _parsers;

        public FirstParser(params IParser<TInput, TOutput>[] parsers)
        {
            Assert.ArrayNotNullAndContainsNoNulls(parsers, nameof(parsers));
            _parsers = parsers;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            if (_parsers.Count == 0)
                return state.Fail(this, "No parsers given");

            for (int i = 0; i < _parsers.Count - 1; i++)
            {
                var parser = _parsers[i];
                var result = parser.Parse(state);
                if (result.Success)
                    return result;
            }

            return _parsers[_parsers.Count - 1].Parse(state);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }
}