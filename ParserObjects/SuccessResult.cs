﻿namespace ParserObjects
{
    /// <summary>
    /// An IParseResult which represents a success
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public struct SuccessResult<TOutput> : IParseResult<TOutput>
    {
        public SuccessResult(TOutput value, Location location)
        {
            Value = value;
            Location = location;
        }

        public bool Success => true;

        public TOutput Value { get; }

        public Location Location { get; }

        public IParseResult<object> Untype() => new SuccessResult<object>(Value, Location);
    }
}