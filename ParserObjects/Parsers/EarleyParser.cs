using System;
using System.Collections.Generic;
using ParserObjects.Earley;
using ParserObjects.Utility;
using ParserObjects.Visitors;

namespace ParserObjects.Parsers;

/// <summary>
/// Implementation of the Earley parsing algorithm.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Earley<TInput, TOutput>
{
    public static IMultiParser<TInput, TOutput> Setup(Func<Earley<TInput, TOutput>.SymbolFactory, INonterminal<TInput, TOutput>> setup)
    {
        var factory = new SymbolFactory(new Dictionary<string, ISymbol>());
        var startNonterminal = setup(factory) ?? throw new GrammarException("Setup callback did not return a valid start symbol");
        return new Parser(startNonterminal);
    }

    public struct SymbolFactory
    {
        private readonly Dictionary<string, ISymbol> _symbols;

        public SymbolFactory(Dictionary<string, ISymbol> symbols)
        {
            _symbols = symbols;
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

    public sealed class Parser : IMultiParser<TInput, TOutput>
    {
        private readonly Engine<TInput, TOutput> _engine;
        private readonly INonterminal<TInput, TOutput> _startSymbol;

        public Parser(INonterminal<TInput, TOutput> startSymbol, string? name = null)
        {
            _engine = new Engine<TInput, TOutput>(startSymbol);
            Name = name ?? startSymbol.Name;
            _startSymbol = startSymbol;
        }

        public string Name { get; }

        public IEnumerable<IParser> GetChildren()
        {
            var visitor = new ChildParserListVisitor();
            return visitor.Visit(_startSymbol);
        }

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCheckpoint = state.Input.Checkpoint();
            var startLocation = state.Input.CurrentLocation;

            var results = _engine.Parse(state);

            startCheckpoint.Rewind();
            return new MultiResult<TOutput>(this, startLocation, startCheckpoint, results.Alternatives, new[] { results.Statistics });
        }

        public override string ToString() => DefaultStringifier.ToString(this);

        public string GetBnf(BnfStringifyVisitor visitor, BnfStringifyVisitor.State state)
        {
            var grammarVisitor = new BnfGrammarVisitor();
            var bnf = grammarVisitor.Visit(_startSymbol, visitor, state);
            return $"EARLEY(\n{bnf}\n)";
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state);

        public INamed SetName(string name) => new Parser(_startSymbol, name);
    }
}
