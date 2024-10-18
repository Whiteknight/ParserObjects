using System;
using ParserObjects.Earley;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Create a new Earley parser. Specify the grammar in the callback and return a reference
    /// to the start symbol.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Earley<TOutput>(Func<EarleySymbolFactory<TInput, TOutput>, INonterminal<TInput, TOutput>> setup)
        => Earley<TInput, TOutput>.Setup(setup);
}
