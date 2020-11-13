using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a delegate to perform the parse
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FuncParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly ParserFunction<TInput, TOutput> _func;

        public FuncParser(ParserFunction<TInput, TOutput> func)
        {
            Assert.ArgumentNotNull(func, nameof(func));
            _func = func;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var checkpoint = t.Input.Checkpoint();
            try
            {
                SuccessFunction<TOutput> onSuccess = (v, l) => Result.Success(v, l ?? t.Input.CurrentLocation);
                FailFunction<TOutput> onFailure = l => Result.Fail<TOutput>(l ?? t.Input.CurrentLocation);
                var result = _func(t, onSuccess, onFailure);
                if (!result.Success)
                    checkpoint.Rewind();
                return result;
            }
            catch
            {
                checkpoint.Rewind();
                return t.Fail<TOutput>();
            }
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
