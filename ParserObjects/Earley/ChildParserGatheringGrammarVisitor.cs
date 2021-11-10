using System.Collections.Generic;

namespace ParserObjects.Earley
{
    public class ChildParserGatheringGrammarVisitor
    {
        private class State
        {
            public State()
            {
                SeenItems = new HashSet<object>();
                ChildParsers = new List<IParser>();
            }

            public HashSet<object> SeenItems { get; }
            public List<IParser> ChildParsers { get; }
        }

        public IEnumerable<IParser> Visit(INonterminal item)
        {
            var state = new State();
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
