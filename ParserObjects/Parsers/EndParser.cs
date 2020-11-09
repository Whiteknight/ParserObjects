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

        public IResult<object> ParseUntyped(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.IsAtEnd
                ? Result.Success<object>(true, t.CurrentLocation)
                : Result.Fail<object>(t.CurrentLocation);
        }

        public IResult<bool> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            return t.IsAtEnd
                ? Result.Success<bool>(true, t.CurrentLocation)
                : Result.Fail<bool>(t.CurrentLocation);
        }
    }
}
