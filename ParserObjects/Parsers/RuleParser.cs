using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parses a list of steps in sequence and produces a single output as a combination of outputs
    /// from each step. Succeeds or fails as an atomic unit.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class RuleParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;
        private readonly Func<IReadOnlyList<object>, TOutput> _produce;

        public RuleParser(IReadOnlyList<IParser<TInput>> parsers, Func<IReadOnlyList<object>, TOutput> produce)
        {
            Assert.ArgumentNotNull(parsers, nameof(parsers));
            Assert.ArgumentNotNull(produce, nameof(produce));
            _parsers = parsers;
            _produce = produce;
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var location = state.Input.CurrentLocation;

            var outputs = new object[_parsers.Count];
            int consumed = 0;
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(state);
                if (result.Success)
                {
                    outputs[i] = result.Value;
                    consumed += result.Consumed;
                    continue;
                }

                checkpoint.Rewind();
                var name = _parsers[i].Name;
                if (string.IsNullOrEmpty(name))
                    name = "(Unnamed)";
                return state.Fail(this, $"Parser {i} {name} failed", result.Location);
            }

            return state.Success(this, _produce(outputs), consumed, location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => _parsers;

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
