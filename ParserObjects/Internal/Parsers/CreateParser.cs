using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

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
    public sealed record Parser(
        Func<IParseState<TInput>, IParser<TInput, TOutput>> GetParser,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Get the parser. The callback has access to the input, so it may consume items.
            // If so, we have to properly report that.
            var startCheckpoint = state.Input.Checkpoint();
            var parser = GetParser(state) ?? throw new InvalidOperationException("Create parser value must not be null");
            var consumedDuringCreation = state.Input.Consumed - startCheckpoint.Consumed;

            var result = parser.Parse(state);

            // If it's a failure result, make sure we are rewound to the beginning and return
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            // If no inputs were consumed during parser creation, we can just return the result
            if (consumedDuringCreation != 0)
                return result.AdjustConsumed(result.Consumed + consumedDuringCreation);

            return result;
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            // Get the parser. The callback has access to the input, so it may consume items.
            // If so, we have to properly report that.
            var startCheckpoint = state.Input.Checkpoint();
            var parser = GetParser(state) ?? throw new InvalidOperationException("Create parser value must not be null");

            var result = parser.Match(state);

            // If it's a failure result, make sure we are rewound to the beginning and return
            if (!result)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString("Create", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    public sealed record MultiParser(
        Func<IParseState<TInput>, IMultiParser<TInput, TOutput>> GetParser,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Get the parser. The callback has access to the input, so it may consume items.
            // If so, we have to properly report that.
            var startCheckpoint = state.Input.Checkpoint();
            var parser = GetParser(state) ?? throw new InvalidOperationException("Create parser value must not be null");
            var consumedDuringCreation = state.Input.Consumed - startCheckpoint.Consumed;

            var result = parser.Parse(state);

            // TODO: In theory the multiparser should rewind the the input sequence to the start
            // point anyway, whether an input was consumed here or not. Double-check the design
            // on this.

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

        public override string ToString() => DefaultStringifier.ToString("Create", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}
