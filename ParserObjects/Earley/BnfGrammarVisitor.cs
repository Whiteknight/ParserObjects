using System.Collections.Generic;
using System.Linq;
using ParserObjects.Visitors;

namespace ParserObjects.Earley;

/// <summary>
/// Extension to BnfStringifyVisitor for producing pseudo-bnf for an Earley grammar.
/// </summary>
public sealed class BnfGrammarVisitor
{
    private record struct State(
        BnfStringifyVisitor Visitor,
        BnfStringifyVisitor.State OuterState,
        HashSet<object> SeenItems,
        List<string> Lines
    );

    public string Visit(INonterminal rootRule, BnfStringifyVisitor visitor, BnfStringifyVisitor.State outerState)
    {
        var state = new State(visitor, outerState, new HashSet<object>(), new List<string>());
        Visit(rootRule, state);
        return string.Join("\n", state.Lines);
    }

    private void Visit(object grammarItem, State sb)
    {
        if (sb.SeenItems.Contains(grammarItem))
            return;

        sb.SeenItems.Add(grammarItem);

        if (grammarItem is IParser t)
            Accept(t, sb);
        if (grammarItem is INonterminal n)
            Accept(n, sb);
        if (grammarItem is IProduction p)
            Accept(p, sb);
    }

    private static void Accept(IParser terminal, State state)
    {
        state.Visitor.VisitChild(terminal, state.OuterState, false);
    }

    private void Accept(INonterminal nonterminal, State state)
    {
        foreach (var production in nonterminal.Productions)
            Visit(production, state);
        foreach (var production in nonterminal.Productions)
        {
            var productionBnf = string.Join(" ", production.Symbols.OfType<INamed>().Select(i => $"<{i.Name}>"));
            state.Lines.Add($"   {nonterminal.Name} := {productionBnf}");
        }
    }

    private void Accept(IProduction production, State state)
    {
        foreach (var s in production.Symbols)
            Visit(s, state);
    }
}
