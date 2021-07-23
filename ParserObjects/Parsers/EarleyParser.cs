using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Earley;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public static class Earley<TInput, TOutput>
    {
        public static IMultiParser<TInput, TOutput> Setup(Func<Earley<TInput, TOutput>.SymbolFactory, INonterminal<TInput, TOutput>> setup)
        {
            var factory = new SymbolFactory();
            var startNonterminal = setup(factory) ?? throw new GrammarException("Setup callback did not return a valid start symbol");
            // TODO: Do we need to validate the grammar at all? Check for symbols which are not linked
            // to the start symbol at all?
            return new Parser(startNonterminal);
        }

        public class SymbolFactory
        {
            private readonly Dictionary<string, ISymbol> _symbols;

            public SymbolFactory()
            {
                _symbols = new Dictionary<string, ISymbol>();
            }

            public INonterminal<TInput, TOutput> New()
                => AllocateNewSymbol<TOutput>(GetNewName());

            public INonterminal<TInput, TOutput> New(string name)
                => AllocateNewSymbol<TOutput>(name);

            public INonterminal<TInput, TValue> New<TValue>()
                => AllocateNewSymbol<TValue>(GetNewName());

            public INonterminal<TInput, TValue> New<TValue>(string name)
                => AllocateNewSymbol<TValue>(name);

            private string GenerateNewName() => $"_S{UniqueIntegerGenerator.GetNext()}";

            private string GetNewName()
            {
                var name = GenerateNewName();
                while (_symbols.ContainsKey(name))
                    name = GenerateNewName();
                return name;
            }

            private INonterminal<TInput, TValue> AllocateNewSymbol<TValue>(string name)
            {
                if (_symbols.ContainsKey(name))
                    throw new GrammarException($"This grammar already contains a symbol named '{name}'");
                var newSymbol = new Nonterminal<TInput, TValue>(name);
                _symbols.Add(name, newSymbol);
                return newSymbol;
            }
        }

        private class MultiResult : IMultiResult<TOutput>
        {
            private readonly IParseStatistics _statistics;

            public MultiResult(IParser parser, Location location, ISequenceCheckpoint startCheckpoint, IEnumerable<IResultAlternative<TOutput>> results, IParseStatistics statistics)
            {
                Parser = parser;
                Results = results.ToList();
                Success = Results.Count(r => r.Success) > 0;
                Location = location;
                StartCheckpoint = startCheckpoint;
                _statistics = statistics;
            }

            public IParser Parser { get; }

            public bool Success { get; }

            public Location Location { get; }

            public IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

            public ISequenceCheckpoint StartCheckpoint { get; }

            public IOption<T> TryGetData<T>()
            {
                if (_statistics is T tStats)
                    return new SuccessOption<T>(tStats);
                return FailureOption<T>.Instance;
            }
        }

        public class Parser : IMultiParser<TInput, TOutput>
        {
            private readonly Engine<TInput, TOutput> _engine;
            private readonly INonterminal<TInput, TOutput> _startSymbol;

            public Parser(INonterminal<TInput, TOutput> startSymbol)
            {
                _engine = new Engine<TInput, TOutput>(startSymbol);
                Name = startSymbol.Name;
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

                // TODO: Do we need to rewind to the startCheckpoint here, if any continuation will
                // rewind to the continuation checkpoint of the alternative?
                startCheckpoint.Rewind();
                return new MultiResult(this, startLocation, startCheckpoint, results.Alternatives, results.Statistics);
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
