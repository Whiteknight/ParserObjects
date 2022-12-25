using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers to transform the result value of an inner parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Transform<TInput, TMiddle, TOutput>
{
    public sealed record Parser(
        IParser<TInput, TMiddle> Inner,
        Func<TMiddle, TOutput> Transform,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            // Execute the parse and transform the result
            var result = Inner.Parse(state);
            if (!result.Success)
                return new FailureResult<TOutput>(result.Parser, result.Location, result.ErrorMessage, default);

            var transformedValue = Transform(result.Value);

            return state.Success(this, transformedValue, result.Consumed, result.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state) => Inner.Match(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    public sealed record MultiParser(
        IMultiParser<TInput, TMiddle> Inner,
        Func<TMiddle, TOutput> Transform,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            // Execute the parse and transform the result
            var result = Inner.Parse(state);
            result.StartCheckpoint.Rewind();

            return result.Transform(Transform);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}
