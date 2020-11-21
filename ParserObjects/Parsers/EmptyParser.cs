using ParserObjects.Utility;
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

        public IResult<object> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            return state.Success(this, null);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
