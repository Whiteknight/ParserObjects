using System;
using ParserObjects.Internal.Parsers;
using ParserObjects.Pratt;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Creates a Pratt parser, which is a precidence-climbing parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Pratt<TOutput>(Action<Configuration<TInput, TOutput>> setup)
        => PrattParser<TInput, TOutput>.Configure(setup);
}
