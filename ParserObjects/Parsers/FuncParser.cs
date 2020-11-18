using System;
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

        public Result<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var checkpoint = t.Input.Checkpoint();
            try
            {
                SuccessFunction<TOutput> onSuccess = (value, loc) => t.Success(this, value, loc ?? t.Input.CurrentLocation);
                FailFunction<TOutput> onFailure = (err, loc) => t.Fail(this, err, loc ?? t.Input.CurrentLocation);
                var result = _func(t, onSuccess, onFailure);
                if (!result.Success)
                    checkpoint.Rewind();
                return result;
            }
            catch (Exception e)
            {
                checkpoint.Rewind();
                t.Log("Unhandled exception while executing Function callback: " + e.Message + "\n\n" + e.StackTrace);
                return t.Fail(this, "Unhandled exception while executing Function callback: " + e.Message);
            }
        }

        public Result<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
