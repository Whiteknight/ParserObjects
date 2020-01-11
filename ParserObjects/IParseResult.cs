using System;

namespace ParserObjects
{
    public interface IParseResult<out TOutput>
    {
        bool Success { get; }
        TOutput Value { get; }
        Location Location { get; }

        IParseResult<object> Untype();
    }

    public static class ParseResultExtensions
    {
        public static IParseResult<TOutput> Transform<TInput, TOutput>(this IParseResult<TInput> input, Func<TInput, TOutput> transform)
        {
            if (input.Success)
                return new SuccessResult<TOutput>(transform(input.Value), input.Location);
            return new FailResult<TOutput>(input.Location);
        }
    }
}