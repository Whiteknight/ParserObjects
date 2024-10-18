using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

public static class Production
{
    /// <summary>
    /// Create a new Production.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="data"></param>
    /// <param name="reduce"></param>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IProduction<TOutput> Create<TData, TOutput>(INonterminal lhs, TData data, Func<TData, object[], TOutput> reduce, params ISymbol[] symbols)
        => new Production<TData, TOutput>(lhs, data, reduce, symbols);
}

public sealed class Production<TData, TOutput> : IProduction<TOutput>
{
    private readonly TData _data;
    private readonly Func<TData, object[], TOutput> _reduce;

    public Production(INonterminal lhs, TData data, Func<TData, object[], TOutput> reduce, IReadOnlyList<ISymbol> symbols)
    {
        Assert.ArgumentNotNull(lhs);
        Assert.ArgumentNotNull(reduce);
        Assert.ArrayNotNullAndContainsNoNulls(symbols);
        Symbols = symbols;
        _reduce = reduce;
        LeftHandSide = lhs;
        _data = data;
    }

    public IReadOnlyList<ISymbol> Symbols { get; }

    public INonterminal LeftHandSide { get; }

    public Option<object> Apply(object[] argsList)
    {
        Assert.ArgumentNotNull(argsList);
        try
        {
            Debug.Assert(argsList.Length >= Symbols.Count, "The arguments buffer should hold at least as many values as there are symbols");
            var value = _reduce(_data, argsList);
            return value == null ? default : new Option<object>(true, value);
        }
        catch
        {
            return default;
        }
    }

    public override string ToString()
        => $"{LeftHandSide.Name} := {string.Join(" ", Symbols.Select(s => s.Name))}";
}
