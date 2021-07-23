using System.Collections.Generic;

namespace ParserObjects
{
    public interface IResultAlternative<TOutput>
    {
        bool Success { get; }
        string ErrorMessage { get; }
        TOutput Value { get; }
        int Consumed { get; }
        ISequenceCheckpoint Continuation { get; }
    }

    public interface IMultiResult : IResultBase
    {
        ISequenceCheckpoint StartCheckpoint { get; }
    }

    public interface IMultiResult<TOutput> : IMultiResult
    {
        IReadOnlyList<IResultAlternative<TOutput>> Results { get; }
    }

    public static class MultiResultExtensions
    {
        public static IResult<TOutput> ToResult<TOutput>(this IMultiResult<TOutput> result, IResultAlternative<TOutput> alt)
        {
            if (alt.Success)
                return new SuccessResult<TOutput>(result.Parser, alt.Value, result.Location, alt.Consumed);
            return new FailResult<TOutput>(result.Parser, result.Location, alt.ErrorMessage);
        }
    }
}
