using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

// Nonterminal contains a list of productions which represent alternative possibilities. In
// this sense, Nonterminal is the Earley-world equivalent of First()
// A Production is an ordered list of symbols which must be matched in order and contain a
// derivation callback. Production is the Earley-world equivalent of Rule()
public sealed class Nonterminal<TInput, TOutput> : INonterminal<TInput, TOutput>
{
    private readonly HashSet<IProduction<TOutput>> _productions;

    public Nonterminal(string name)
    {
        _productions = new HashSet<IProduction<TOutput>>();
        Name = string.IsNullOrEmpty(name) ? $"N{UniqueIntegerGenerator.GetNext()}" : name;
    }

    private Nonterminal(IEnumerable<IProduction<TOutput>> productions, string name)
    {
        _productions = new HashSet<IProduction<TOutput>>(productions);
        Name = name;
    }

    public IReadOnlyCollection<IProduction> Productions => _productions;

    public void Add(IProduction<TOutput> p)
    {
        if (!_productions.Contains(p))
            _productions.Add(p);
    }

    public bool Contains(IProduction p)
        => p is IProduction<TOutput> typed && _productions.Contains(typed);

    public string Name { get; }

    public override string ToString() => string.Join("\n", _productions.Select(p => p.ToString()));

    public INamed SetName(string name) => new Nonterminal<TInput, TOutput>(_productions, name);
}
