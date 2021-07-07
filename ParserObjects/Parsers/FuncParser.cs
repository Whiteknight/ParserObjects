using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a delegate to perform the parse. The delegate may perform any logic necessary and
    /// imposes no particular structure.
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
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var checkpoint = state.Input.Checkpoint();
            try
            {
                IResult<TOutput> OnSuccess(TOutput value, Location? loc)
                    => state.Success(this, value, 0, loc ?? checkpoint.Location);

                IResult<TOutput> OnFailure(string err, Location? loc)
                    => state.Fail(this, err, loc ?? checkpoint.Location);

                var result = _func(state, OnSuccess, OnFailure);
                var totalConsumed = state.Input.Consumed - checkpoint.Consumed;

                if (result.Success)
                    return state.Success(this, result.Value, totalConsumed, result.Location);

                checkpoint.Rewind();
                return result;
            }
            catch (Exception e)
            {
                checkpoint.Rewind();
                state.Log(this, "Unhandled exception while executing Function callback: " + e.Message + "\n\n" + e.StackTrace);
                return state.Fail(this, "Unhandled exception while executing Function callback: " + e.Message);
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
