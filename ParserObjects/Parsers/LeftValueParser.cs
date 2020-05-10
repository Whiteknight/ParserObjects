using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// A parser for holding a parsed result from left application. Do not use this type
    /// directly.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class LeftValueParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public TOutput Value { get; set; }
        public Location Location { get; set; }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => new SuccessResult<TOutput>(Value, Location);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => new SuccessResult<object>(Value, Location);

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}