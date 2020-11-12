namespace ParserObjects
{
    public sealed class ParseState<TInput>
    {
        public ParseState(ISequence<TInput> input, object data)
        {
            Input = input;
            Data = data;
        }

        public ISequence<TInput> Input { get; private set; }

        public object Data { get; set; }

        public IResult<T> Fail<T>() => Result.Fail<T>(Input.CurrentLocation);
        public IResult<T> Success<T>(T value, Location location = null) => Result.Success(value, location ?? Input.CurrentLocation);
    }
}
