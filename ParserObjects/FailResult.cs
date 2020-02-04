using System;

namespace ParserObjects
{
    /// <summary>
    /// An IParseResult which represents a failure
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public struct FailResult<TValue> : IParseResult<TValue>
    {
        public FailResult(Location location)
        {
            Location = location;
        }

        public bool Success => false;

        public TValue Value => default;

        public Location Location { get; }

        public IParseResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
            => new FailResult<TOutput>(Location);
    }
}