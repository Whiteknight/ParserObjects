using System;
using System.Collections.Generic;
using ParserObjects.Earley;
using ParserObjects.Internal.Earley;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

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

public readonly struct EarleySymbolFactory<TInput, TOutput>
{
    private readonly Dictionary<string, ISymbol> _symbols;

    public EarleySymbolFactory(Dictionary<string, ISymbol> symbols)
    {
        _symbols = symbols;
    }

    public INonterminal<TInput, TOutput> New()
        => AllocateNewSymbol<TOutput>(GetNewName());

    public INonterminal<TInput, TOutput> New(string name)
        => AllocateNewSymbol<TOutput>(name);

    public INonterminal<TInput, TValue> New<TValue>()
        => AllocateNewSymbol<TValue>(GetNewName());

    public INonterminal<TInput, TValue> New<TValue>(string name)
        => AllocateNewSymbol<TValue>(name);

    private static string GenerateNewName() => $"_S{UniqueIntegerGenerator.GetNext()}";

    private string GetNewName()
    {
        var name = GenerateNewName();
        while (_symbols.ContainsKey(name))
            name = GenerateNewName();
        return name;
    }

    private INonterminal<TInput, TValue> AllocateNewSymbol<TValue>(string name)
    {
        if (_symbols.ContainsKey(name))
            throw new GrammarException($"This grammar already contains a symbol named '{name}'");
        var newSymbol = new Nonterminal<TInput, TValue>(name);
        _symbols.Add(name, newSymbol);
        return newSymbol;
    }
}
