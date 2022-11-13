﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

public sealed class Production<TValue> : IProduction
{
    private readonly Func<object[], TValue> _reduce;

    public Production(INonterminal lhs, Func<object[], TValue> reduce, params ISymbol[] symbols)
    {
        Symbols = symbols.ToList();
        _reduce = reduce;
        LeftHandSide = lhs;
    }

    public IReadOnlyList<ISymbol> Symbols { get; }

    public INonterminal LeftHandSide { get; }

    public IOption<object> Apply(object[] argsList)
    {
        try
        {
            Debug.Assert(argsList.Length >= Symbols.Count, "The arguments buffer should hold at least as many values as there are symbols");
            var value = _reduce(argsList);
            return value == null ? FailureOption<object>.Instance : new SuccessOption<object>(value);
        }
        catch (ProductionFailedException)
        {
            return FailureOption<object>.Instance;
        }
    }

    public override string ToString()
        => $"{LeftHandSide.Name} := {string.Join(" ", Symbols.Select(s => s.Name))}";
}