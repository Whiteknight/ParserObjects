using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Produces an output value unconditionally. Consumes no input. The callback has access to
    /// both the input sequence and the current contextual data, to help crafting the value.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class ProduceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<ISequence<TInput>, IDataStore, TOutput> _produce;

        public ProduceParser(Func<ISequence<TInput>, IDataStore, TOutput> produce)
        {
            Assert.ArgumentNotNull(produce, nameof(produce));
            _produce = produce;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var value = _produce(state.Input, state.Data);
            return state.Success(this, value, state.Input.CurrentLocation);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}