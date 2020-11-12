using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// The empty parser, consumes no input and always returns success
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EmptyParser<TInput> : IParser<TInput, object>
    {
        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IResult<object> ParseUntyped(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.Success<object>(null);
        }

        public IResult<object> Parse(ParseState<TInput> t) => ParseUntyped(t);
    }
}
