using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// The empty parser, consumes no input and always returns success
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EmptyParser<TInput> : IParser<TInput, object>
    {
        public string Name { get; set; }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Result.Success<object>(null, t?.Input.CurrentLocation);

        public IResult<object> Parse(ParseState<TInput> t) => Result.Success<object>(null, t?.Input.CurrentLocation);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
