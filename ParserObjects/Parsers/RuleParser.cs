using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parses a list of steps in sequence and produces a single output as a combination of outputs of
    /// each step. Succeeds or fails as an atomic unit.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
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
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var location = state.Input.CurrentLocation;

            var outputs = new object[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(state);
                if (result.Success)
                {
                    outputs[i] = result.Value;
                    continue;
                }
                checkpoint.Rewind();
                var name = _parsers[i].Name;
                if (string.IsNullOrEmpty(name))
                    name = "(Unnamed)";
                return state.Fail(this, $"Parser {i} {name} failed", result.Location);
            }
            return state.Success(this, _produce(outputs), location);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

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

            return new RuleParser<TInput, TOutput>(newList, _produce);
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}