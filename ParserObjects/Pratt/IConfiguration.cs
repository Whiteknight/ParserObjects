using System;

namespace ParserObjects.Pratt;

/// <summary>
/// Configuration for Pratt parsers.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IConfiguration<TInput, TOutput>
{
    /// <summary>
    /// Add a parselet using the given parser.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="matcher"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    IConfiguration<TInput, TOutput> Add<TValue>(IParser<TInput, TValue> matcher, Action<IPrattParseletBuilder<TInput, TValue, TOutput>> setup);

    /// <summary>
    /// Add a parselet using the given parser as a null denominator with 0 binding power.
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    IConfiguration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher);

    /// <summary>
    /// Add a reference to a parser which is used by a user callback but not otherwise
    /// referenced by the Pratt parser. This parser reference will be made available through
    /// the GetChildren method, and will be available for visiting.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    IConfiguration<TInput, TOutput> Reference(IParser parser);
}
