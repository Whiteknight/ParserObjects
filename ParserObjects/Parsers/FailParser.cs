using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns unconditional failure
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.Fail<TOutput>();
        }

        IResult<object> IParser<TInput>.ParseUntyped(ParseState<TInput> t) => t.Fail<object>();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}