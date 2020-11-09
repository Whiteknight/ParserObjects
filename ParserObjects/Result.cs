using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static class Result
    {
        public static IResult<TValue> Fail<TValue>(Location location = null)
            => new Result<TValue>(false, default, location);

        public static IResult<TValue> Success<TValue>(TValue value, Location location = null)
            => new Result<TValue>(true, value, location);

        public static IResult<TValue> New<TValue>(bool success, TValue value, Location location = null)
            => new Result<TValue>(success, value, location);
    }

    public class Result<TValue> : IResult<TValue>
    {
        public Result(bool success, TValue value, Location location)
        {
            Success = success;
            Value = value;
            Location = location;
        }

        public bool Success { get; }

        public TValue Value { get; }

        public Location Location { get; }

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            if (!Success)
                return new Result<TOutput>(false, default, Location);
            var newValue = transform(Value);
            return new Result<TOutput>(true, newValue, Location);
        }
    }
}