using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Parsers;

namespace ParserObjects
{
    /// <summary>
    /// IParser extension methods for building combinators using fluent syntax.
    /// </summary>
    public static class ParserCombinatorExtensions
    {
        /// <summary>
        /// Execute a parser and use the result to select the next parser to invoke.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getNext"></param>
        /// <param name="mentions"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Chain<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Chain<TInput, TMiddle, TOutput>.GetParser getNext, params IParser[] mentions)
            => new Chain<TInput, TMiddle, TOutput>.Parser(p, getNext, mentions);

        public static IParser<TInput, TOutput> ChainWith<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Action<Chain<TInput, TMiddle, TOutput>.IConfiguration> setup)
            => new Chain<TInput, TMiddle, TOutput>.Parser(p, setup);

        /// <summary>
        /// Execute a parser but consume no input. Use the result to select the next parser to
        /// invoke.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getNext"></param>
        /// <param name="mentions"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Choose<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> p, Chain<TInput, TMiddle, TOutput>.GetParser getNext, params IParser[] mentions)
            => new Chain<TInput, TMiddle, TOutput>.Parser(None(p), getNext, mentions);

        /// <summary>
        /// Invoke callbacks before and after a parse.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Examine<TInput, TOutput>(this IParser<TInput, TOutput> parser, Action<Examine<TInput, TOutput>.Context> before = null, Action<Examine<TInput, TOutput>.Context> after = null)
            => new Examine<TInput, TOutput>.Parser(parser, before, after);

        /// <summary>
        /// Invoke callbacks before and after a parse.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static IParser<TInput> Examine<TInput>(this IParser<TInput> parser, Action<Examine<TInput>.Context> before = null, Action<Examine<TInput>.Context> after = null)
            => new Examine<TInput>.Parser(parser, before, after);

        /// <summary>
        /// Zero-length assertion that the given parser's result is followed by another sequence.
        /// The lookahead sequence is matched but not consumed.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> FollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
            => ParserMethods<TInput>.Combine(p, ParserMethods<TInput>.PositiveLookahead(lookahead)).Transform(r => (TOutput)r[0]);

        /// <summary>
        /// Returns a list of results from the given parser. Continues to parse until the parser returns
        /// failure. Returns an enumerable of results.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, bool atLeastOne)
            => new LimitedListParser<TInput, TOutput>(p, atLeastOne ? 1 : 0, null);

        /// <summary>
        /// Returns a list of results from the given parser, with limits. Continues to
        /// parse until the parser returns failure or the maximum number of results is
        /// reached.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> List<TInput, TOutput>(this IParser<TInput, TOutput> p, int minimum = 0, int? maximum = null)
            => new LimitedListParser<TInput, TOutput>(p, minimum, maximum);

        /// <summary>
        /// Given a parser which parses characters, parse a list of characters and return the sequence as a
        /// string.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<char, string> ListCharToString(this IParser<char, char> p, bool atLeastOne)
            => p.List(atLeastOne).Transform(c => new string(c.ToArray()));

        /// <summary>
        /// Given a parser which parsers characters, parse a list of characters and return
        /// the result as a string. Supports limits for minimum and maximum numbers of
        /// characters to parse.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static IParser<char, string> ListCharToString(this IParser<char, char> p, int minimum = 0, int? maximum = null)
            => p.List(minimum, maximum).Transform(c => new string(c.ToArray()));

        /// <summary>
        /// Returns a list of results from the given parser separated by a separator pattern. Continues until
        /// the item or separator parser return failure. Returns an enumerable of results.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> ListSeparatedBy<TInput, TSeparator, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput, TSeparator> separator, bool atLeastOne)
            => ParserMethods<TInput>.SeparatedList(p, separator, atLeastOne);

        /// <summary>
        /// Returns a list of results from the given parser separated by a separator
        /// pattern. Continues until the item or separator pattern return failure, or
        /// the minimum/maximum counts are not satisfied. Returns an enumeration of results.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TOutput>> ListSeparatedBy<TInput, TSeparator, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput, TSeparator> separator, int minimum = 0, int? maximum = null)
            => ParserMethods<TInput>.SeparatedList(p, separator, minimum, maximum);

        /// <summary>
        /// Given a parser which parses strings, parse a list of strings and return the sequence as a joined
        /// string.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<char, string> ListStringsToString(this IParser<char, string> p, bool atLeastOne = false)
            => p.List(atLeastOne).Transform(s => string.Join(string.Empty, s));

        /// <summary>
        /// Transform the output of the given parser. Synonym for Transform.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Map<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);

        /// <summary>
        /// Wraps the given parser to guarantee that it consumes no input.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> None<TInput, TOutput>(this IParser<TInput, TOutput> inner)
            => new NoneParser<TInput, TOutput>(inner);

        /// <summary>
        /// Wraps the given parser to guarantee that it consumes no input.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static IParser<TInput> None<TInput>(this IParser<TInput> inner)
            => new NoneParser<TInput>(inner);

        /// <summary>
        /// Zero-length assertion that the given parser's match result is not followed by a lookahead pattern.
        /// The lookahead is compared but no input is consumed to match it.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> NotFollowedBy<TInput, TOutput>(this IParser<TInput, TOutput> p, IParser<TInput> lookahead)
            => ParserMethods<TInput>.Combine(p, ParserMethods<TInput>.NegativeLookahead(lookahead)).Transform(r => (TOutput)r[0]);

        /// <summary>
        /// The results of the given parser are optional. If the given parser fails, a default value will
        /// be provided.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
            => ParserMethods<TInput>.First(p, ParserMethods<TInput>.Produce(getDefault ?? (() => default)));

        /// <summary>
        /// The results of the given parser are optiona. If the given parser fails, a default value will be
        /// provided.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> getDefault)
            => ParserMethods<TInput>.First(p, ParserMethods<TInput>.Produce(getDefault ?? (t => default)));

        /// <summary>
        /// Make this parser replaceable.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IParser<TInput, TOutput> p)
            => new ReplaceableParser<TInput, TOutput>(p);

        /// <summary>
        /// Make this parser replaceable. Gives the parser a name so that it can be easily
        /// found and replaced.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
            => new ReplaceableParser<TInput, TOutput>(p).Named(name);

        /// <summary>
        /// Transform the output of the given parser to a new value.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
    }
}
