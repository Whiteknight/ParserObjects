using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns unconditional failure, optionally with a helpful error message.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Fail<TInput, TOutput>
    {
        public class Parser : IParser<TInput, TOutput>
        {
            public Parser(string? errorMessage = null)
            {
                ErrorMessage = errorMessage ?? "Guaranteed fail";
                Name = string.Empty;
            }

            public string Name { get; set; }
            public string ErrorMessage { get; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                return state.Fail(this, ErrorMessage);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        public class MultiParser : IMultiParser<TInput, TOutput>
        {
            public MultiParser(string? errorMessage = null)
            {
                ErrorMessage = errorMessage ?? "Guaranteed fail";
                Name = string.Empty;
            }

            public string Name { get; set; }
            public string ErrorMessage { get; }

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startCheckpoint = state.Input.Checkpoint();
                return new MultiResult<TOutput>(this, state.Input.CurrentLocation, startCheckpoint, new[]
                {
                    new FailureResultAlternative<TOutput>(ErrorMessage, startCheckpoint)
                });
            }

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
