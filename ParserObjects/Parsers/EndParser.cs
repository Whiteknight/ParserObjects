using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches at the end of the input sequence
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EndParser<TInput> : IParser<TInput, object>
    {
        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) 
            => t.IsAtEnd 
                ? new SuccessResult<object>(null, t.CurrentLocation) 
                : (IParseResult<object>) new FailResult<object>(t.CurrentLocation);

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);
    }
}
