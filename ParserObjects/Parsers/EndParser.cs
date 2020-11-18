using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches at the end of the input sequence. Fails if the input sequence is at any point besides the
    /// end.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EndParser<TInput> : IParser<TInput, bool>
    {
        public string Name { get; set; }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IResult<bool> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.Input.IsAtEnd
                ? t.Success(this, true)
                : t.Fail(this, "Expected end of Input but found " + t.Input.Peek().ToString());
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
