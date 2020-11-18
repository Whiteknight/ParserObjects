using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Parser base type.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// The name of the parser. This value is only used for bookkeeping information and does not have
        /// an affect on the parse.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get a list of child parsers, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IParser> GetChildren();

        /// <summary>
        /// Return a parser exactly like this parser but with one of it's children replaced. If no
        /// replacement is made, this parser may be returned. 
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        IParser ReplaceChild(IParser find, IParser replace);
    }

    /// <summary>
    /// Parser object which allows getting the result without type information
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IParser<TInput> : IParser
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result of type object. If the parse
        /// fails, it is expected that this method will return the input sequence to the state it was at
        /// before the parse was attempted.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        IResult Parse(ParseState<TInput> t);
    }

    /// <summary>
    /// Parser with explicit input and output types.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParser<TInput, out TOutput> : IParser<TInput>
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result. If the parse fails, it is
        /// expected that this method will return the input sequence to the state it was at before the
        /// parse was attempted.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        new IResult<TOutput> Parse(ParseState<TInput> t);
    }

    /// <summary>
    /// A parser which has an in-place replaceable child. Used to identify parsers which can participate in
    /// certain find/replace operations
    /// </summary>
    public interface IReplaceableParserUntyped : IParser
    {
        /// <summary>
        /// The child parser which can be replaced without cloning
        /// </summary>
        IParser ReplaceableChild { get; }

        /// <summary>
        /// Set the new child parser without cloning
        /// </summary>
        /// <param name="parser"></param>
        SingleReplaceResult SetParser(IParser parser);
    }
}