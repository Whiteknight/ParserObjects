using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects
{
    public interface IMultiResultAlternative<TOutput>
    {
        bool Success { get; }
        string ErrorMessage { get; }
        TOutput Value { get; }
        int Consumed { get; }
        ISequenceCheckpoint Continuation { get; }
    }

    public interface IMultiResult<TOutput>
    {
        // The parser which generated the multiresult
        IParser Parser { get; }

        // Whether there is at least one successful result
        bool Success { get; }

        // The start location of all possible results
        Location Location { get; }

        ISequenceCheckpoint StartCheckpoint { get; }

        IReadOnlyList<IMultiResultAlternative<TOutput>> Results { get; }
    }

    public class MultiResult<TOutput> : IMultiResult<TOutput>
    {
        public MultiResult(IParser parser, Location location, ISequenceCheckpoint startCheckpoint, IEnumerable<IMultiResultAlternative<TOutput>> results)
        {
            Parser = parser;
            Results = results.ToList();
            Success = Results.Count(r => r.Success) > 0;
            Location = location;
            StartCheckpoint = startCheckpoint;
        }

        public IParser Parser { get; }

        public bool Success { get; }

        public Location Location { get; }

        public IReadOnlyList<IMultiResultAlternative<TOutput>> Results { get; }

        public ISequenceCheckpoint StartCheckpoint { get; }
    }

    public class SuccessMultiResultAlternative<TOutput> : IMultiResultAlternative<TOutput>
    {
        public SuccessMultiResultAlternative(TOutput value, int consumed, ISequenceCheckpoint continuation)
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
    }

    public class FailureMultiResultAlternative<TOutput> : IMultiResultAlternative<TOutput>
    {
        public FailureMultiResultAlternative(string errorMessage, ISequenceCheckpoint continuation)
        {
            ErrorMessage = errorMessage;
            Continuation = continuation;
        }

        public bool Success => false;

        public string ErrorMessage { get; }

        public TOutput Value => throw new InvalidOperationException("This result is not a success and does not have a valid value.");

        public int Consumed => 0;

        public ISequenceCheckpoint Continuation { get; }
    }

    public static class MultiResultExtensions
    {
        public static IResult<TOutput> ToResult<TOutput>(this IMultiResult<TOutput> result, IMultiResultAlternative<TOutput> alt)
        {
            if (alt.Success)
                return new SuccessResult<TOutput>(result.Parser, alt.Value, result.Location, alt.Consumed);
            return new FailResult<TOutput>(result.Parser, result.Location, alt.ErrorMessage);
        }
    }
}
