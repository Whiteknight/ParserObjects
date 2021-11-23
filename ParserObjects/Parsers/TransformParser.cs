using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Parsers to transform the raw result object, including the result type, result value,
/// success flag or error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Transform<TInput, TMiddle, TOutput>
{
    public record struct SingleArguments(IParser<TInput, TOutput> Parser, IParseState<TInput> State, IResult<TMiddle> Result, ISequenceCheckpoint StartCheckpoint)
    {
        public ISequence<TInput> Input => State.Input;
        public IDataStore Data => State.Data;
        public IResult<TOutput> Failure(string errorMessage, Location? location = null)
            => State.Fail(Parser, errorMessage, location ?? State.Input.CurrentLocation);

        public IResult<TOutput> Success(TOutput value, Location? location = null)
            => State.Success(Parser, value, State.Input.Consumed - StartCheckpoint.Consumed, location ?? State.Input.CurrentLocation);
    }

    public record struct MultiArguments(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State, IMultiResult<TMiddle> Result, ISequenceCheckpoint StartCheckpoint);

    public class Parser : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _inner;
        private readonly Func<SingleArguments, IResult<TOutput>> _transform;

        public Parser(IParser<TInput, TMiddle> inner, Func<SingleArguments, IResult<TOutput>> transform)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(transform, nameof(transform));
            _inner = inner;
            _transform = transform;
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            // Execute the parse and transform the result
            var result = _inner.Parse(state);
            var args = new SingleArguments(this, state, result, startCheckpoint);
            var transformedResult = _transform(args);

            // If the transform callback returns failure, see if we have to rewind input and
            // then return directly (we don't need to calculate consumed or anything)
            if (!transformedResult.Success)
            {
                if (result.Success)
                    startCheckpoint.Rewind();
                return transformedResult;
            }

            // Make sure that the transformed result is reporting the correct number of
            // consumed inputs (the transformer didn't secretly consume some without properly
            // accounting for them)
            var totalConsumed = state.Input.Consumed - startCheckpoint.Consumed;
            if (transformedResult.Consumed != totalConsumed)
                return state.Success(this, transformedResult.Value, totalConsumed, transformedResult.Location);

            return transformedResult;
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class MultiParser : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly Func<MultiArguments, IMultiResult<TOutput>> _transform;

        public MultiParser(IMultiParser<TInput, TMiddle> inner, Func<MultiArguments, IMultiResult<TOutput>> transform)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(transform, nameof(transform));
            _inner = inner;
            _transform = transform;
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            // Execute the parse and transform the result
            var result = _inner.Parse(state);
            result.StartCheckpoint.Rewind();
            var beforeTransformConsumed = state.Input.Consumed;
            var args = new MultiArguments(this, state, result, startCheckpoint);
            var transformedResult = _transform(args);
            var afterTransformConsumed = state.Input.Consumed;
            var totalTransformConsumed = afterTransformConsumed - beforeTransformConsumed;
            totalTransformConsumed = totalTransformConsumed < 0 ? 0 : totalTransformConsumed;

            // If the transform callback returns failure, we've already rewound the input so
            // just return. Otherwise if the transform consumed nothing, return the result
            if (!transformedResult.Success || totalTransformConsumed == 0)
                return transformedResult;

            // Things get messy because we have to double-check all the .Consumed values to make
            // sure they are correct. It's better if the transform doesn't consume input, but
            // we can't enforce that here (yet)
            return transformedResult.Recreate((alt, factory) =>
            {
                var totalConsumed = alt.Continuation.Consumed - startCheckpoint.Consumed;
                if (totalConsumed <= 0)
                    return alt;
                return factory(alt.Value, totalConsumed, alt.Continuation);
            });
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
