using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers to transform the raw result object, including the result type, result value,
/// success flag or error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Transform<TInput, TMiddle, TOutput>
{
    public record struct SingleArguments(IParser<TInput, TOutput> Parser, IParseState<TInput> State, IResult<TMiddle> Result, SequenceCheckpoint StartCheckpoint)
    {
        public ISequence<TInput> Input => State.Input;
        public IDataStore Data => State.Data;
        public IResult<TOutput> Failure(string errorMessage, Location? location = null)
            => State.Fail(Parser, errorMessage, location ?? State.Input.CurrentLocation);

        public IResult<TOutput> Success(TOutput value, Location? location = null)
            => State.Success(Parser, value, State.Input.Consumed - StartCheckpoint.Consumed, location ?? State.Input.CurrentLocation);
    }

    public record struct MultiArguments(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State, IMultiResult<TMiddle> Result, SequenceCheckpoint StartCheckpoint);

    public sealed record Parser(
        IParser<TInput, TMiddle> Inner,
        Func<SingleArguments, IResult<TOutput>> Transform,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Possible Results:
            // 1. The inner parser succeeds, Transform succeeds. Return success, with this parser listed as the source
            // 2. The inner parser succeeds, Transform fails. Return failure, with this parser listed as the source
            // 2. The inner parser fails. Return failure directly, with the inner parser listed as source

            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            // Execute the parse and transform the result
            var result = Inner.Parse(state);
            var args = new SingleArguments(this, state, result, startCheckpoint);
            var transformedResult = Transform(args);

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
            return transformedResult.AdjustConsumed(totalConsumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        // NOTE: It is not possible to optimize this. The user callback function can make any
        // arbitrary transformation to a result, including turning a success into a failure or
        // vice-versa, or consuming input. That means just calling Inner.Match() might return the
        // wrong value, and might consume the wrong number of inputs. Therefore we must do a complete
        // parse here (because the transform callback requires the Inner.Parse() result)
        public bool Match(IParseState<TInput> state) => Parse(state).Success;

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    public sealed record MultiParser(
        IMultiParser<TInput, TMiddle> Inner,
        Func<MultiArguments, IMultiResult<TOutput>> Transform,
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
            var beforeTransformConsumed = state.Input.Consumed;
            var args = new MultiArguments(this, state, result, startCheckpoint);
            var transformedResult = Transform(args);
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

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}
