using System.Collections.Generic;
using ParserObjects.Earley;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public static class Earley<TInput, TOutput>
    {
        public class SymbolFactory
        {
            public INonterminal<TInput, TOutput> New()
                => new Nonterminal<TInput, TOutput>($"S{UniqueIntegerGenerator.GetNext()}");

            public INonterminal<TInput, TOutput> New(string name)
                => new Nonterminal<TInput, TOutput>(name);

            public INonterminal<TInput, TValue> New<TValue>(string name)
                => new Nonterminal<TInput, TValue>(name);
        }

        public class Parser : IMultiParser<TInput, TOutput>
        {
            private readonly Engine<TInput, TOutput> _engine;
            private readonly INonterminal<TInput, TOutput> _startSymbol;

            public Parser(INonterminal<TInput, TOutput> startSymbol)
            {
                _engine = new Engine<TInput, TOutput>(startSymbol);
                Name = $"S{UniqueIntegerGenerator.GetNext()}";
                _startSymbol = startSymbol;
            }

            public string Name { get; set; }

            public IEnumerable<IParser> GetChildren()
            {
                var visitor = new ChildParserGatheringGrammarVisitor();
                return visitor.Visit(_startSymbol);
            }

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                var startCheckpoint = state.Input.Checkpoint();
                var startLocation = state.Input.CurrentLocation;
                var results = _engine.Parse(state);
                startCheckpoint.Rewind();
                return new MultiResult<TOutput>(this, startLocation, startCheckpoint, results);
            }

            public override string ToString() => DefaultStringifier.ToString(this);

            public string GetBnf()
            {
                var visitor = new BnfGrammarVisitor();
                return "(\n" + visitor.Visit(_startSymbol) + "\n);";
            }
        }
    }
}
