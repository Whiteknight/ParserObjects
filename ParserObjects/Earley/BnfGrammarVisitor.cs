using System.Collections.Generic;
using System.Linq;
using ParserObjects;

namespace ParserObjects.Earley
{
    public class BnfGrammarVisitor
    {
        private class State
        {
            public State()
            {
                SeenItems = new HashSet<object>();
                Lines = new List<string>();
            }

            public HashSet<object> SeenItems { get; }
            public List<string> Lines { get; }
        }

        public string Visit(INonterminal rootRule)
        {
            var state = new State();
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

        private void Accept(IParser terminal, State state)
        {
            // TODO: recurse into the terminal to get the parser's BNF
            state.Lines.Add($"{terminal.Name} := parser");
        }

        private void Accept(INonterminal nonterminal, State state)
        {
            foreach (var production in nonterminal.Productions)
                Visit(production, state);
            foreach (var production in nonterminal.Productions)
                state.Lines.Add(nonterminal.Name + " := " + string.Join(" ", production.Symbols.OfType<INamed>().Select(i => $"<{i.Name}>")));
        }

        private void Accept(IProduction production, State state)
        {
            foreach (var s in production.Symbols)
                Visit(s, state);
        }
    }
}
