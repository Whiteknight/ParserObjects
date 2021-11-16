using System.Collections.Generic;

namespace ParserObjects.Earley
{
    /// <summary>
    /// Visitor to get references to all child parsers referenced by the grammar.
    /// </summary>
    public class ChildParserListVisitor
    {
        private record struct State(HashSet<object> SeenItems, List<IParser> ChildParsers);

        public IEnumerable<IParser> Visit(INonterminal item)
        {
            var state = new State(new HashSet<object>(), new List<IParser>());
            Visit(item, state);
            return state.ChildParsers;
        }

        private void Visit(object item, State state)
        {
            if (state.SeenItems.Contains(item))
                return;
            state.SeenItems.Add(item);

            if (item is IParser t)
                Accept(t, state);
            if (item is INonterminal n)
                Accept(n, state);
            if (item is IProduction p)
                Accept(p, state);
        }

        private static void Accept(IParser terminal, State state)
        {
            state.ChildParsers.Add(terminal);
        }

        private void Accept(INonterminal nonterminal, State state)
        {
            foreach (var production in nonterminal.Productions)
                Visit(production, state);
        }

        private void Accept(IProduction production, State state)
        {
            foreach (var symbol in production.Symbols)
                Visit(symbol, state);
        }
    }
}
