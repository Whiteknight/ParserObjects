namespace ParserObjects
{
    /// <summary>
    /// State information about parse, including the input sequence, logging and contextual data.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IParseState<out TInput>
    {
        IDataStore Data { get; }

        ISequence<TInput> Input { get; }

        IResultsCache Cache { get; }

        void Log(IParser parser, string message);
    }

    public static class ParseStateExtensions
    {
        public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser parser, string error, Location location)
        {
            state.Log(parser, "Failed with error " + error);
            return new FailResult<TOutput>(parser, location, error);
        }

        public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, string error)
            => Fail(state, parser, error, state.Input.CurrentLocation);

        public static IResult<TOutput> Fail<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, string error, Location location)
        {
            state.Log(parser, "Failed with error " + error);
            return new FailResult<TOutput>(parser, location, error);
        }

        public static IResult Fail<TInput>(this IParseState<TInput> state, IParser<TInput> parser, string error, Location location)
        {
            state.Log(parser, "Failed with error " + error);
            return new FailResult<object>(parser, location, error);
        }

        public static IResult Fail<TInput>(this IParseState<TInput> state, IParser<TInput> parser, string error)
            => Fail(state, parser, error, state.Input.CurrentLocation);

        public static IResult<TOutput> Success<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, TOutput output, int consumed, Location location)
        {
            state.Log(parser, "Succeeded");
            return new SuccessResult<TOutput>(parser, output, location, consumed);
        }

        public static IResult<TOutput> Success<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, TOutput output, int consumed)
            => Success(state, parser, output, consumed, state.Input.CurrentLocation);

        public static IResult<object> Success<TInput>(this IParseState<TInput> state, IParser<TInput> parser, object output, int consumed, Location location)
        {
            state.Log(parser, "Succeeded");
            return new SuccessResult<object>(parser, output, location, consumed);
        }

        public static IResult<object> Success<TInput>(this IParseState<TInput> state, IParser<TInput> parser, object output, int consumed)
            => Success(state, parser, output, consumed, state.Input.CurrentLocation);

        public static IResult<TOutput> Result<TInput, TOutput>(this IParseState<TInput> state, IParser<TInput, TOutput> parser, IPartialResult<TOutput> part)
        {
            if (part.Success)
                return Success(state, parser, part.Value, part.Consumed, part.Location);
            return Fail(state, parser, part.ErrorMessage, part.Location);
        }
    }
}
