using System;

namespace ParserObjects
{
    public sealed class ParseState<TInput>
    {
        private Action<string> _logCallback;

        public ParseState(ISequence<TInput> input, object data, Action<string> logCallback = null)
        {
            Input = input;
            Data = data;
            _logCallback = logCallback;
        }

        public ISequence<TInput> Input { get; }

        public object Data { get; set; }

        public void Log(string message)
            => _logCallback?.Invoke(message);

        public IResult<TOutput> Fail<TOutput>(IParser<TInput, TOutput> parser, string error, Location location = null)
        {
            return new Result<TOutput>(parser, false, default, location ?? Input.CurrentLocation, error);
        }

        public IResult<object> FailUntyped(IParser<TInput> parser, string error, Location location = null)
        {
            return new Result<object>(parser, false, default, location ?? Input.CurrentLocation, error);
        }

        public IResult<TOutput> Success<TOutput>(IParser<TInput, TOutput> parser, TOutput output, Location location = null)
        {
            return new Result<TOutput>(parser, true, output, location ?? Input.CurrentLocation, null);
        }
    }
}
