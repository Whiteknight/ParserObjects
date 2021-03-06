﻿using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Result object representing success. This result contains a valid value and metadata about
    /// that value.
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
        public string ErrorMessage => string.Empty;
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

    /// <summary>
    /// Result object representing failure. This result contains an error message and information
    /// about the location of the error. Attempting to access the Value will result in an
    /// exception.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class FailResult<TValue> : IResult<TValue>
    {
        public FailResult(IParser parser, Location location, string message)
        {
            Parser = parser;
            Location = location;
            ErrorMessage = message;
        }

        public IParser Parser { get; }
        public bool Success => false;
        public TValue Value => throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);
        public Location Location { get; }
        public string ErrorMessage { get; }
        public int Consumed => 0;
        object IResult.Value => Value!;

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            return new FailResult<TOutput>(Parser, Location, ErrorMessage);
        }

        public override string ToString() => $"{Parser} FAIL at {Location}";
    }

    /// <summary>
    /// A partial result representing failure. This result contains an error message and information
    /// about the error. Attempting to access the Value property will trigger an exception.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class FailurePartialResult<TValue> : IPartialResult<TValue>
    {
        public FailurePartialResult(string error, Location location)
        {
            Location = location;
            ErrorMessage = error;
        }

        public bool Success => false;
        public TValue Value => throw new InvalidOperationException("This result does not have a successful value");

        public int Consumed => 0;
        public Location Location { get; }

        public string ErrorMessage { get; }
    }

    /// <summary>
    /// A partial result representing success. This result contains a valid result value and
    /// metadata about that value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SuccessPartialResult<TValue> : IPartialResult<TValue>
    {
        public SuccessPartialResult(TValue value, int consumed, Location location)
        {
            Value = value;
            Consumed = consumed;
            Location = location;
        }

        public bool Success => true;
        public TValue Value { get; }

        public int Consumed { get; }
        public Location Location { get; }

        public string ErrorMessage => string.Empty;
    }
}
