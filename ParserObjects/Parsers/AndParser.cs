using System.Collections.Generic;
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
            Assert.ArrayNotNullAndContainsNoNulls(parsers, nameof(parsers));
            _parsers = parsers;
        }

        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startLocation = state.Input.CurrentLocation;
            var checkpoint = state.Input.Checkpoint();
            int consumed = 0;
            foreach (var parser in _parsers)
            {
                var result = parser.Parse(state);
                if (!result.Success)
                {
                    checkpoint.Rewind();
                    return result;
                }

                consumed += result.Consumed;
            }

            return state.Success(this, null, consumed, startLocation);
        }

        public IEnumerable<IParser> GetChildren() => _parsers;

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
