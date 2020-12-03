﻿using System;
using ParserObjects.Sequences;

namespace ParserObjects
{
    /// <summary>
    /// General-purpose extensions for IParser and descendents.
    /// </summary>
    public static class ParserMatchParseExtensions
    {
        /// <summary>
        /// Attempts a parse but does not consume any input. Instead it returns a boolean true if the parse
        /// succeeded or false otherwise.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TInput>(this IParser<TInput> parser, ISequence<TInput> input)
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
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch(this IParser<char> parser, string input)
        {
            // Don't need to .Checkpoint()/.Rewind() because the sequence is private and we don't
            // reuse it
            var sequence = new StringCharacterSequence(input);
            var state = new ParseState<char>(sequence, null);
            var result = parser.Parse(state);
            return result.Success;
        }

        /// <summary>
        /// Convenience method for parser which act on character sequences. Parse the given input string
        /// and return the first value or failure. Creates a character sequence from the string
        /// and the ParseState object.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="s"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IResult<TOutput> Parse<TOutput>(this IParser<char, TOutput> parser, string s, Action<string> log = null)
            => parser.Parse(new ParseState<char>(new StringCharacterSequence(s), log));

        /// <summary>
        /// Convenience method to invoke a parser which acts on an input sequence. Creates the
        /// necessary ParseState.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IResult<TOutput> Parse<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input, Action<string> log = null)
            => parser.Parse(new ParseState<TInput>(input, log));

        /// <summary>
        /// Covenience method to invoke a parser which does not return a value. Creates the
        /// necessary ParseState.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IResult Parse<TInput>(this IParser<TInput> parser, ISequence<TInput> input, Action<string> log = null)
            => parser.Parse(new ParseState<TInput>(input, log));
    }
}