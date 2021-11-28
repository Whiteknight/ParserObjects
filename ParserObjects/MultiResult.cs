using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// The output from a multi-parser which may contain multiple result values representing
    /// multiple parallel attempts.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class MultiResult<TOutput> : IMultiResult<TOutput>
    {
        public MultiResult(IParser parser, Location location, ISequenceCheckpoint startCheckpoint, IEnumerable<IResultAlternative<TOutput>> results)
        {
            Parser = parser;
            Results = results.ToList();
            Success = Results.Any(r => r.Success);
            Location = location;
            StartCheckpoint = startCheckpoint;
        }

        public IParser Parser { get; }

        public bool Success { get; }

        public Location Location { get; }

        public IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

        public ISequenceCheckpoint StartCheckpoint { get; }

        IReadOnlyList<IResultAlternative> IMultiResult.Results => Results;

        public IMultiResult<TOutput> Recreate(Func<IResultAlternative<TOutput>, ResultAlternativeFactoryMethod<TOutput>, IResultAlternative<TOutput>> recreate, IParser? parser = null, ISequenceCheckpoint? startCheckpoint = null, Location? location = null)
        {
            Assert.ArgumentNotNull(recreate, nameof(recreate));
            var newAlternatives = Results.Select(alt => !alt.Success ? alt : recreate(alt, alt.Factory));
            var newCheckpoint = startCheckpoint ?? StartCheckpoint;
            var newLocation = location ?? Location;
            return new MultiResult<TOutput>(Parser, newLocation, newCheckpoint, newAlternatives);
        }

        public IMultiResult<TValue> Transform<TValue>(Func<TOutput, TValue> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            var newAlternatives = Results.Select(alt => alt.Transform(transform));
            return new MultiResult<TValue>(Parser, Location, StartCheckpoint, newAlternatives);
        }
    }

    /// <summary>
    /// Result value which represents a single success, including information necessary to continue
    /// the parse from the point of the success.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class SuccessResultAlternative<TOutput> : IResultAlternative<TOutput>
    {
        public SuccessResultAlternative(TOutput value, int consumed, ISequenceCheckpoint continuation)
        {
            Assert.ArgumentNotNull(continuation, nameof(continuation));
            Value = value;
            Consumed = consumed;
            Continuation = continuation;
        }

        public static IResultAlternative<TOutput> FactoryMethod(TOutput value, int consumed, ISequenceCheckpoint continuation)
            => new SuccessResultAlternative<TOutput>(value, consumed, continuation);

        public bool Success => true;

        public string ErrorMessage => string.Empty;

        public TOutput Value { get; }

        public int Consumed { get; }

        public ISequenceCheckpoint Continuation { get; }

        public ResultAlternativeFactoryMethod<TOutput> Factory => FactoryMethod;

        object IResultAlternative.Value => Value!;

        public IResultAlternative<TValue> Transform<TValue>(Func<TOutput, TValue> transform)
        {
            var newValue = transform(Value);
            return new SuccessResultAlternative<TValue>(newValue, Consumed, Continuation);
        }
    }

    /// <summary>
    /// Result value which represents a single failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class FailureResultAlternative<TOutput> : IResultAlternative<TOutput>
    {
        public FailureResultAlternative(string errorMessage, ISequenceCheckpoint continuation)
        {
            ErrorMessage = errorMessage;
            Continuation = continuation;
        }

        public bool Success => false;

        public string ErrorMessage { get; }

        public TOutput Value => throw new InvalidOperationException("This result is not a success and does not have a valid value.");

        object IResultAlternative.Value => throw new InvalidOperationException("This result is not a success and does not have a valid value.");

        public int Consumed => 0;

        public ISequenceCheckpoint Continuation { get; }

        public ResultAlternativeFactoryMethod<TOutput> Factory => throw new InvalidOperationException("This result is not a success and does not have a factory");

        public IResultAlternative<TValue> Transform<TValue>(Func<TOutput, TValue> transform)
            => new FailureResultAlternative<TValue>(ErrorMessage, Continuation);
    }
}
