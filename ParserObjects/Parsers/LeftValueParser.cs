using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// A parser for holding a parsed result from left application. Do not use this type
    /// directly.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class LeftValueParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private Result<TOutput> _result;

        public void SetResult(Result<TOutput> result)
        {
            _result = result;
        }

        public Result<TOutput> Parse(ParseState<TInput> t) => _result;

        Result<object> IParser<TInput>.ParseUntyped(ParseState<TInput> t) => _result.Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}