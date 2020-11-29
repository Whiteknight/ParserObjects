using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a parser and inverses the result success status. If the parser succeeds, return 
    /// Failure. Otherwise returns Success. Consumes no input in either case and returns no output.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NotParser<TInput> : IParser<TInput>
    {
        private readonly IParser<TInput> _inner;

        public NotParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var checkpoint = state.Input.Checkpoint();
            var result1 = _inner.Parse(state);
            if (result1.Success)
            {
                checkpoint.Rewind();
                return state.Fail(this, "Parser matched but was not supposed to");
            }

            return state.Success(this, null, result1.Location);
        }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };
    }
}