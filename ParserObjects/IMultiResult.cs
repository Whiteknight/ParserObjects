using System;
using System.Collections.Generic;

namespace ParserObjects
{
    public interface IResultAlternative
    {
        bool Success { get; }
        string ErrorMessage { get; }
        object Value { get; }
        int Consumed { get; }
        ISequenceCheckpoint Continuation { get; }
    }

    public delegate IResultAlternative<TOutput> ResultAlternativeFactoryMethod<TOutput>(TOutput value, int consumed, ISequenceCheckpoint continuation);

    public interface IResultAlternative<TOutput> : IResultAlternative
    {
        new TOutput Value { get; }

        ResultAlternativeFactoryMethod<TOutput> Factory { get; }

        IResultAlternative<TValue> Transform<TValue>(Func<TOutput, TValue> transform);
    }

    public interface IMultiResult : IResultBase
    {
        ISequenceCheckpoint StartCheckpoint { get; }

        IReadOnlyList<IResultAlternative> Results { get; }
    }

    public interface IMultiResult<TOutput> : IMultiResult
    {
        new IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

        IMultiResult<TOutput> Recreate(Func<IResultAlternative<TOutput>, ResultAlternativeFactoryMethod<TOutput>, IResultAlternative<TOutput>> recreate, IParser? parser = null, ISequenceCheckpoint? startCheckpoint = null, Location? location = null);

        IMultiResult<TValue> Transform<TValue>(Func<TOutput, TValue> transform);
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
