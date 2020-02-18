using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches any input item that isn't the end of input. Consumes exactly one input item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnyParser<T> : IParser<T, T>
    {
        // TODO: We can replace this with MatchPredicate(x => true)

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParseResult<object> ParseUntyped(ISequence<T> t) => Parse(t).Untype();

        public IParseResult<T> Parse(ISequence<T> t)
        {
            if (t.IsAtEnd)
                return new FailResult<T>();
            var next = t.GetNext();
            return new SuccessResult<T>(next, t.CurrentLocation);
        }
    }
}