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

            private static string GenerateNewName() => $"_S{UniqueIntegerGenerator.GetNext()}";

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

        private class MultiResult : IMultiResult<TOutput>, IResultHasData
        {
            private readonly IParseStatistics _statistics;

            public MultiResult(IParser parser, Location location, ISequenceCheckpoint startCheckpoint, IEnumerable<IResultAlternative<TOutput>> results, IParseStatistics statistics)
            {
                Parser = parser;
                Results = results.ToList();
                Success = Results.Any(r => r.Success);
                Location = location;
                StartCheckpoint = startCheckpoint;
                _statistics = statistics;
            }

            public IParser Parser { get; }

            public bool Success { get; }

            public Location Location { get; }

            public IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

            public ISequenceCheckpoint StartCheckpoint { get; }

            IReadOnlyList<IResultAlternative> IMultiResult.Results => Results;

            public IMultiResult<TOutput> Recreate(Func<IResultAlternative<TOutput>, ResultAlternativeFactoryMethod<TOutput>, IResultAlternative<TOutput>> recreate, IParser? parser = null, ISequenceCheckpoint? startCheckpoint = null, Location? location = null)
            {
                Assert.ArgumentNotNull(recreate, nameof(recreate));
                var newAlternatives = Results.Select(alt =>
                {
                    if (!alt.Success)
                        return alt;
                    return recreate(alt, alt.Factory);
                });
                var newCheckpoint = startCheckpoint ?? StartCheckpoint;
                var newLocation = location ?? Location;
                return new MultiResult(Parser, newLocation, newCheckpoint, newAlternatives, _statistics);
            }

            public IMultiResult<TValue> Transform<TValue>(Func<TOutput, TValue> transform)
            {
                var newAlternatives = Results.Select(alt => alt.Transform(transform));
                return new Earley<TInput, TValue>.MultiResult(Parser, Location, StartCheckpoint, newAlternatives, _statistics);
            }

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

                startCheckpoint.Rewind();
                return new MultiResult(this, startLocation, startCheckpoint, results.Alternatives, results.Statistics);
            }

            public override string ToString() => DefaultStringifier.ToString(this);

            public string GetBnf()
            {
                var visitor = new BnfGrammarVisitor();
                return "(\n" + visitor.Visit(_startSymbol) + "\n);";
            }

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
                => Parse(state);
        }
    }
}
