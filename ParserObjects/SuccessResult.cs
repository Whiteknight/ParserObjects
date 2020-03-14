using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// An IParseResult which represents a success. This result will have a valid Value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public struct SuccessResult<TValue> : IParseResult<TValue>
    {
        public SuccessResult(TValue value, Location location)
        {
            Value = value;
            Location = location;
        }

        public bool Success => true;

        public TValue Value { get; }

        public Location Location { get; }

        public IParseResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            var newValue = transform(Value);
            return new SuccessResult<TOutput>(newValue, Location);
        }
    }
}