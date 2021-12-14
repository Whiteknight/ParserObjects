using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Create a new parser instance on invocation, using information available from the current
/// parse state, and invokes it.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Create<TInput, TOutput>
{
    /// <summary>
    /// Create a parser dynamically using information from the parse state. The parser created is
    /// not expected to be constant and will not be cached.
    /// </summary>
    public sealed class Parser : IParser<TInput, TOutput>
    {
        private readonly Func<IParseState<TInput>, IParser<TInput, TOutput>> _getParser;

        public Parser(Func<IParseState<TInput>, IParser<TInput, TOutput>> getParser, string name = "")
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
            Name = name;
        }

        public string Name { get; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Get the parser. The callback has access to the input, so it may consume items.
            // If so, we have to properly report that.
            var startCheckpoint = state.Input.Checkpoint();
            var parser = _getParser(state) ?? throw new InvalidOperationException("Create parser value must not be null");
            var consumedDuringCreation = state.Input.Consumed - startCheckpoint.Consumed;

            var result = parser.Parse(state);

            // If it's a failure result, make sure we are rewound to the beginning and return
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            // If no inputs were consumed during parser creation, we can just return the result
            if (consumedDuringCreation == 0)
                return result;

            // Otherwise construct a new result with correct consumed value.
            return state.Success(this, result.Value, result.Consumed + consumedDuringCreation, result.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => new Parser(_getParser, name);
    }

    public sealed class MultiParser : IMultiParser<TInput, TOutput>
    {
        private readonly Func<IParseState<TInput>, IMultiParser<TInput, TOutput>> _getParser;

        public MultiParser(Func<IParseState<TInput>, IMultiParser<TInput, TOutput>> getParser, string name = "")
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
            Name = name;
        }

        public string Name { get; }

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Get the parser. The callback has access to the input, so it may consume items.
            // If so, we have to properly report that.
            var startCheckpoint = state.Input.Checkpoint();
            var parser = _getParser(state) ?? throw new InvalidOperationException("Create parser value must not be null");
            var consumedDuringCreation = state.Input.Consumed - startCheckpoint.Consumed;

            var result = parser.Parse(state);

            // If it's a failure result, make sure we are rewound to the beginning and return
            if (!result.Success)
            {
                if (consumedDuringCreation > 0)
                    startCheckpoint.Rewind();
                return result;
            }

            // If no inputs were consumed during parser creation, we can just return the result
            if (consumedDuringCreation == 0)
                return result;

            return result.Recreate((alt, factory) => factory(alt.Value, alt.Consumed + consumedDuringCreation, alt.Continuation), parser: this, startCheckpoint: startCheckpoint);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => new MultiParser(_getParser, name);
    }
}
