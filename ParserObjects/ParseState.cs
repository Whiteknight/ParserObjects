using System;
using ParserObjects.Utility;

namespace ParserObjects
{
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

        public ISequence<TInput> Input { get; }

        public IDataStore Data => _store;

        public void Log(string message) => _logCallback?.Invoke(message);

        public IResult<TOutput> Fail<TOutput>(IParser<TInput, TOutput> parser, string error, Location location = null)
            => new Result<TOutput>(parser, false, default, location ?? Input.CurrentLocation, error);

        public IResult Fail(IParser<TInput> parser, string error, Location location = null)
            => new Result<object>(parser, false, default, location ?? Input.CurrentLocation, error);

        public IResult<TOutput> Success<TOutput>(IParser<TInput, TOutput> parser, TOutput output, Location location = null)
            => new Result<TOutput>(parser, true, output, location ?? Input.CurrentLocation, null);

        public IResult<object> Success(IParser<TInput> parser, object output, Location location = null)
            => new Result<object>(parser, true, output, location ?? Input.CurrentLocation, null);
    }
}
