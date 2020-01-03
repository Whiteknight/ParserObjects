using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parses a list of steps and produces a single output. Succeeds or fails as an atomic unit
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class RuleParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput>> _parsers;
        private readonly Func<IReadOnlyList<object>, TOutput> _produce;

        public RuleParser(IReadOnlyList<IParser<TInput>> parsers, Func<IReadOnlyList<object>, TOutput> produce)
        {
            _parsers = parsers;
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            var window = new WindowSequence<TInput>(t);
            var outputs = new object[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].ParseUntyped(window);
                if (!result.Success)
                {
                    window.Rewind();
                    return new FailResult<TOutput>(result.Location);
                }

                outputs[i] = result.Value;
            }
            return new SuccessResult<TOutput>(_produce(outputs), location);
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parsers.Contains(find) && replace is IParser<TInput> realReplace)
            {
                var newList = new IParser<TInput>[_parsers.Count];
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var child = _parsers[i];
                    newList[i] = child == find ? realReplace : child;
                }

                return new RuleParser<TInput, TOutput>(newList, _produce);
            }

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}