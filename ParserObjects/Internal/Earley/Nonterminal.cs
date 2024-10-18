using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

// Nonterminal contains a list of productions which represent alternative possibilities. In
// this sense, Nonterminal is the Earley-world equivalent of First()
// A Production is an ordered list of symbols which must be matched in order and contain a
// derivation callback. Production is the Earley-world equivalent of Rule()
public sealed class Nonterminal<TInput, TOutput> : HashSet<IProduction<TOutput>>, INonterminal<TInput, TOutput>
{
    public Nonterminal(string name)
    {
        Name = string.IsNullOrEmpty(name) ? $"N{UniqueIntegerGenerator.GetNext()}" : name;
    }

    private Nonterminal(IEnumerable<IProduction<TOutput>> productions, string name)
        : base(productions)
    {
        Name = name;
    }

    public IReadOnlyCollection<IProduction> Productions => this;

    public new void Add(IProduction<TOutput> p)
    {
        if (!Contains(p))
            base.Add(p);
    }

    public string Name { get; }

    public override string ToString() => string.Join("\n", this.Select(p => p.ToString()));

    public INamed SetName(string name) => new Nonterminal<TInput, TOutput>(this, name);
}
