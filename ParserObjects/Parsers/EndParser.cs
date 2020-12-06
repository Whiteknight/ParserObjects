using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches at the end of the input sequence. Fails if the input sequence is at any point
    /// besides the end.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EndParser<TInput> : IParser<TInput>
    {
        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            return state.Input.IsAtEnd
                ? state.Success(this, true)
                : state.Fail(this, "Expected end of Input but found " + state.Input.Peek().ToString());
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
