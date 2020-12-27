using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Create a new parser instance on invocation, using information available from the current
    /// parse state, and invokes it.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Create<TInput, TOutput>
    {
        public delegate IParser<TInput, TOutput> Function(ISequence<TInput> input, IDataStore data);

        /// <summary>
        /// Create a parser dynamically using information from the parse state. The parser created is
        /// not expected to be constant and will not be cached.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Function _getParser;

            public Parser(Function getParser)
            {
                Assert.ArgumentNotNull(getParser, nameof(getParser));
                _getParser = getParser;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                // Get the parser. The callback has access to the input, so it may consume items.
                // If so, we have to properly report that.
                var startConsumed = state.Input.Consumed;
                var parser = _getParser(state.Input, state.Data) ?? throw new InvalidOperationException("Create parser value must not be null");
                var consumedDuringCreation = state.Input.Consumed - startConsumed;

                var result = parser.Parse(state);

                // We can safely return the result if it's a fail or if it is correctly reporting
                // the number of consumed inputs
                if (!result.Success || result.Consumed == consumedDuringCreation)
                    return result;

                // Otherwise construct a new result with correct consumed value.
                return state.Success(this, result.Value, result.Consumed + consumedDuringCreation, result.Location);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
