using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Bnf;

/// <summary>
/// Parser-visitor to traverse the parser-graph and attempt to produce a string of approximate
/// pseudo-BNF to describe the grammar. Proper execution of this visitor depends on parsers having
/// the .Name value set. If you have custom parser types you can create a subclass of this visitor
/// type with signature 'public state.Visit(MyParserType parser, State state)' and it should dispatch to
/// them as required.
/// </summary>
public sealed class BnfStringifier
{
    // Uses a syntax inspired by W3C EBNF (https://www.w3.org/TR/REC-xml/#sec-notation) and Regex
    // (for extensions beyond what EBNF normally handles). This NOT intended for round-trip operations
    // or formal analysis purposes.

    private readonly List<IPartialVisitor<BnfStringifyVisitor>> _partials;

    public BnfStringifier()
    {
        _partials = new List<IPartialVisitor<BnfStringifyVisitor>>
        {
            new BuiltInTypesBnfStringifyVisitor()
        };
    }

    public static BnfStringifier Instance { get; } = new BnfStringifier();

    public void Add(IPartialVisitor<BnfStringifyVisitor> partial)
    {
        if (partial == null || _partials.Contains(partial))
            return;
        _partials.Add(partial);
    }

    public void Add<T>()
        where T : IPartialVisitor<BnfStringifyVisitor>, new()
    {
        if (_partials.Any(p => p is T))
            return;
        Add(new T());
    }

    public string Stringify(IParser parser)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        var sb = new StringBuilder();
        var state = new BnfStringifyVisitor(sb, _partials);
        state.Visit(parser);
        return sb.ToString();
    }
}
