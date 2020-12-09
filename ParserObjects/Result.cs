﻿using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Parser result object which holds the result value and helpful metadata.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SuccessResult<TValue> : IResult<TValue>
    {
        public SuccessResult(IParser parser, TValue value, Location location, int consumed)
        {
            Parser = parser;
            Value = value;
            Location = location;
            Consumed = consumed;
        }

        public IParser Parser { get; }
        public bool Success => true;
        public TValue Value { get; }
        public Location Location { get; }
        public string Message => string.Empty;
        public int Consumed { get; }
        object IResult.Value => Value!;

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            var newValue = transform(Value);
            return new SuccessResult<TOutput>(Parser, newValue, Location, Consumed);
        }

        public override string ToString() => $"{Parser} Ok at {Location}";
    }

    public class FailResult<TValue> : IResult<TValue>
    {
        public FailResult(IParser parser, Location location, string message)
        {
            Parser = parser;
            Location = location;
            Message = message;
        }

        public IParser Parser { get; }
        public bool Success => false;
        public TValue Value => throw new InvalidOperationException("This result has failed. There is no value to access");
        public Location Location { get; }
        public string Message { get; }
        public int Consumed => 0;
        object IResult.Value => Value!;

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            return new FailResult<TOutput>(Parser, Location, Message);
        }

        public override string ToString() => $"{Parser} FAIL at {Location}";
    }
}
