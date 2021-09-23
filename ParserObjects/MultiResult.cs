using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects
{
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

        public IOption<T> TryGetData<T>() => FailureOption<T>.Instance;
    }

    public class SuccessResultAlternative<TOutput> : IResultAlternative<TOutput>
    {
        public SuccessResultAlternative(TOutput value, int consumed, ISequenceCheckpoint continuation)
        {
            Value = value;
            Consumed = consumed;
            Continuation = continuation;
        }

        public bool Success => true;

        public string ErrorMessage => string.Empty;

        public TOutput Value { get; }

        public int Consumed { get; }

        public ISequenceCheckpoint Continuation { get; }

        object IResultAlternative.Value => Value!;
    }

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
    }
}
