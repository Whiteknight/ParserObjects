using System;
using System.Collections.Generic;
using System.Linq;

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
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) 
            => new SuccessResult<TOutput>(_produce(t), t.CurrentLocation);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) 
            => new SuccessResult<object>(_produce(t), t.CurrentLocation);

        public string Name { get; set; }
        
        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParser Accept(IParserVisitor visitor) => (visitor as ICoreVisitorDispatcher)?.VisitProduce(this) ?? this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}