using ParserObjects.Sequences;

namespace ParserObjects
{
    /// <summary>
    /// General-purpose extensions for IParser and descendents
    /// </summary>
    public static class ParserMatchParseExtensions
    {
        /// <summary>
        /// Attempts a parse but does not consume any input. Instead it returns a boolean true if the parse
        /// succeeded or false otherwise.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
        {
            var checkpoint = input.Checkpoint();
            var state = new ParseState<TInput>(input, null);
            var result = parser.Parse(state);
            checkpoint.Rewind();
            return result.Success;
        }

        /// <summary>
        /// Convenience method for parsers which act on character sequences. Attempts a parse but does not
        /// consume any input. Returns true if the parse would succeed, false otherwise.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TOutput>(this IParser<char, TOutput> parser, string input)
            => CanMatch(parser, new StringCharacterSequence(input));

        /// <summary>
        /// Convenience method for parser which act on character sequences. Parse the given input string
        /// and return the first value or failure.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IResult<TOutput> Parse<TOutput>(this IParser<char, TOutput> parser, string s)
            => parser.Parse(new ParseState<char>(new StringCharacterSequence(s)));

        public static IResult<TOutput> Parse<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
            => parser.Parse(new ParseState<TInput>(input));

        public static IResult<object> ParseUntyped<TInput>(this IParser<TInput> parser, ISequence<TInput> input)
            => parser.ParseUntyped(new ParseState<TInput>(input));
    }
}