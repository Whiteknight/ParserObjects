using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns unconditional failure, optionally with a helpful error message.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public FailParser(string? errorMessage = null)
        {
            ErrorMessage = errorMessage ?? "Guaranteed fail";
            Name = string.Empty;
        }

        public string Name { get; set; }
        public string ErrorMessage { get; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            return state.Fail(this, ErrorMessage);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
