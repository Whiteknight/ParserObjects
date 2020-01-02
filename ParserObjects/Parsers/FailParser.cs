using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public IParseResult<TOutput> Parse(ISequence<TInput> t) => new FailResult<TOutput>(t.CurrentLocation);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => new FailResult<object>(t.CurrentLocation);

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