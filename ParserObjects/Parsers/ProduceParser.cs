using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser to produce an output node unconditionally. Consumes no input.
    /// This is used to provide a default node value
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class ProduceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<ISequence<TInput>, TOutput> _produce;

        public ProduceParser(Func<ISequence<TInput>, TOutput> produce)
        {
            Assert.ArgumentNotNull(produce, nameof(produce));
            _produce = produce;
        }

        public IResult<TOutput> Parse(ISequence<TInput> t)
            => Result.Success(_produce(t), t.CurrentLocation);

        IResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t)
            => Result.Success<object>(_produce(t), t.CurrentLocation);

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}