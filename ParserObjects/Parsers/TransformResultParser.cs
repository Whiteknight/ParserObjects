using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Transforms the raw result object, including the result type, result value, success flag or
    /// error message.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput1"></typeparam>
    /// <typeparam name="TOutput2"></typeparam>
    public static class TransformResult<TInput, TOutput1, TOutput2>
    {
        public delegate IResult<TOutput2> Function(ISequence<TInput> input, IDataStore data, IResult<TOutput1> result);

        public class Parser : IParser<TInput, TOutput2>
        {
            private readonly IParser<TInput, TOutput1> _inner;
            private readonly Function _transform;

            public Parser(IParser<TInput, TOutput1> inner, Function transform)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                Assert.ArgumentNotNull(transform, nameof(transform));
                _inner = inner;
                _transform = transform;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput2> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startConsumed = state.Input.Consumed;
                var cp = state.Input.Checkpoint();

                // Execute the parse and transform the result
                var result = _inner.Parse(state);
                var transformedResult = _transform(state.Input, state.Data, result);

                // If the transform callback returns failure, see if we have to rewind input and
                // then return directly (we don't need to calculate consumed or anything)
                if (!transformedResult.Success)
                {
                    if (result.Success)
                        cp.Rewind();
                    return transformedResult;
                }

                // Make sure that the transformed result is reporting the correct number of
                // consumed inputs (the transformer didn't secretly consume some without properly
                // accounting for them)
                var totalConsumed = state.Input.Consumed - startConsumed;
                if (transformedResult.Consumed != totalConsumed)
                    return state.Success(this, transformedResult.Value, totalConsumed, transformedResult.Location);

                return transformedResult;
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
