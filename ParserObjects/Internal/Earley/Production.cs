using System;
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
        Symbols = symbols.ToArray();
        _reduce = reduce;
        LeftHandSide = lhs;
    }

    public IReadOnlyList<ISymbol> Symbols { get; }

    public INonterminal LeftHandSide { get; }

    public Option<object> Apply(object[] argsList)
    {
        // TODO: What should happen if the _reduce callback throws an exception? Bubble it up
        // (which would probably nuke the whole parse) or return default here (possibly with some
        // kind of logging)?
        Debug.Assert(argsList.Length >= Symbols.Count, "The arguments buffer should hold at least as many values as there are symbols");
        var value = _reduce(argsList);
        return value == null ? default : new Option<object>(true, value);
    }

    public override string ToString()
        => $"{LeftHandSide.Name} := {string.Join(" ", Symbols.Select(s => s.Name))}";
}
