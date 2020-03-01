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

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.IsAtEnd
                ? new SuccessResult<object>(true, t.CurrentLocation)
                : (IParseResult<object>) new FailResult<object>(t.CurrentLocation);
        }

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.IsAtEnd
                ? new SuccessResult<bool>(true, t.CurrentLocation)
                : (IParseResult<bool>) new FailResult<bool>(t.CurrentLocation);
        }
    }
}
