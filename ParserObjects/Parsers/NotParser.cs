using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a parser and inverses the result success status. If the parser succeeds, return 
    /// Failure. Otherwise returns Success. Consumes no input in either case and returns no output.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NotParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public NotParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IResult<object> Parse(ParseState<TInput> t) => ParseUntyped(t);

        public IResult<object> ParseUntyped(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var checkpoint = t.Input.Checkpoint();
            var result1 = _inner.ParseUntyped(t);
            if (result1.Success)
            {
                checkpoint.Rewind();
                return t.Fail(this, "Parser matched but was not supposed to");
            }

            return t.Success(this, null, result1.Location);
        }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, bool> typed1)
                return new NotParser<TInput>(typed1);
            return this;
        }
    }
}