using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserObjects.Internal.Bnf;

/// <summary>
/// Parser-visitor to traverse the parser-graph and attempt to produce a string of approximate
/// pseudo-BNF to describe the grammar. Proper execution of this visitor depends on parsers having
/// the .Name value set. If you have custom parser types you can implement
/// IPartialVisitor(BnfStringifyVisitor) and call .Add() to include it in the visit.
/// </summary>
public sealed class BnfStringifier : IVisitor<BnfStringifyState>
{
    private readonly List<IPartialVisitor<BnfStringifyState>> _partials;

    public BnfStringifier()
    {
        _partials = new List<IPartialVisitor<BnfStringifyState>>
        {
            new BuiltInTypesBnfStringifyVisitor()
        };
    }

    public static BnfStringifier Instance { get; } = new BnfStringifier();

    public TPartial? Get<TPartial>()
        where TPartial : IPartialVisitor<BnfStringifyState>
        => _partials.OfType<TPartial>().FirstOrDefault();

    public void Add(IPartialVisitor<BnfStringifyState> partial)
    {
        Assert.ArgumentNotNull(partial);
        if (partial == null || _partials.Contains(partial))
            return;
        _partials.Add(partial);
    }

    public void Add<T>()
        where T : IPartialVisitor<BnfStringifyState>, new()
    {
        if (_partials.Exists(p => p is T))
            return;
        Add(new T());
    }

    public string Stringify(IParser parser)
    {
        Assert.ArgumentNotNull(parser);
        var sb = new StringBuilder();
        var state = new BnfStringifyState(this, sb);
        state.Visit(parser);
        return sb.ToString();
    }
}
