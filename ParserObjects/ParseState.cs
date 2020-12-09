﻿using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Contains state data and metadata about the progress of the parse. Provides references to
    /// useful objects which may affect the parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public sealed class ParseState<TInput>
    {
        private readonly Action<string> _logCallback;
        private readonly CascadingKeyValueStore _store;

        public ParseState(ISequence<TInput> input, Action<string> logCallback = null)
        {
            Input = input;
            _store = new CascadingKeyValueStore();
            _logCallback = logCallback;
        }

        /// <summary>
        /// Gets the input sequence being used by the parser.
        /// </summary>
        public ISequence<TInput> Input { get; }

        /// <summary>
        /// Gets the current contextual state data used by the parser.
        /// </summary>
        public IDataStore Data => _store;

        /// <summary>
        /// If a log callback is provided, pass a log message to the callback.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="message"></param>
        public void Log(IParser parser, string message) => _logCallback?.Invoke($"{parser}: {message}");

        /// <summary>
        /// Create a Failure result for the given parser with the given error information.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="error"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IResult<TOutput> Fail<TOutput>(IParser<TInput, TOutput> parser, string error, Location location = null)
        {
            Log(parser, "Failed with error " + error);
            return new FailResult<TOutput>(parser, location ?? Input.CurrentLocation, error);
        }

        public IResult<TOutput> Fail<TOutput>(IParser parser, string error, Location location = null)
        {
            Log(parser, "Failed with error " + error);
            return new FailResult<TOutput>(parser, location ?? Input.CurrentLocation, error);
        }

        /// <summary>
        /// Create a failure result for the given parser with the given error information.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="error"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IResult Fail(IParser<TInput> parser, string error, Location location = null)
        {
            Log(parser, "Failed with error " + error);
            return new FailResult<object>(parser, location ?? Input.CurrentLocation, error);
        }

        /// <summary>
        /// Create a success result for the given parser with the given result value.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="output"></param>
        /// <param name="consumed"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IResult<TOutput> Success<TOutput>(IParser<TInput, TOutput> parser, TOutput output, int consumed, Location location = null)
        {
            Log(parser, "Succeeded");
            return new SuccessResult<TOutput>(parser, output, location ?? Input.CurrentLocation, consumed);
        }

        /// <summary>
        /// Create a success result for the given parser with the given result value, if any.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="output"></param>
        /// <param name="consumed"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IResult<object> Success(IParser<TInput> parser, object output, int consumed, Location location = null)
        {
            Log(parser, "Succeeded");
            return new SuccessResult<object>(parser, output, location ?? Input.CurrentLocation, consumed);
        }
    }
}
