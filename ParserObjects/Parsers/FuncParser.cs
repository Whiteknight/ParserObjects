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
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var checkpoint = state.Input.Checkpoint();
            try
            {
                IResult<TOutput> onSuccess(TOutput value, Location loc = null) 
                    => state.Success(this, value, loc ?? state.Input.CurrentLocation);
                IResult<TOutput> onFailure(string err, Location loc = null) 
                    => state.Fail(this, err, loc ?? state.Input.CurrentLocation);
                var result = _func(state, onSuccess, onFailure);
                if (!result.Success)
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

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }
}
