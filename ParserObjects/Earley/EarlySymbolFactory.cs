using System.Collections.Generic;
using ParserObjects.Internal;
using ParserObjects.Internal.Earley;

namespace ParserObjects.Earley;

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

    private Nonterminal<TInput, TValue> AllocateNewSymbol<TValue>(string name)
    {
        if (_symbols.ContainsKey(name))
            throw new GrammarException($"This grammar already contains a symbol named '{name}'");
        var newSymbol = new Nonterminal<TInput, TValue>(name);
        _symbols.Add(name, newSymbol);
        return newSymbol;
    }
}
