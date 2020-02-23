using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns unconditional failure
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public IParseResult<TOutput> Parse(ISequence<TInput> t) 
            => new FailResult<TOutput>(t.CurrentLocation);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) 
            => new FailResult<object>(t.CurrentLocation);

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParser Accept(IParserVisitor visitor) => (visitor as ICoreVisitorDispatcher)?.VisitFail(this) ?? this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}