using System;
using System.Collections.Generic;
using ParserObjects.Earley;
using ParserObjects.Internal.Bnf;
using ParserObjects.Internal.Earley;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Implementation of the Earley parsing algorithm.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Earley<TInput, TOutput>
{
    public static IMultiParser<TInput, TOutput> Setup(Func<EarleySymbolFactory<TInput, TOutput>, INonterminal<TInput, TOutput>> setup)
    {
        var factory = new EarleySymbolFactory<TInput, TOutput>(new Dictionary<string, ISymbol>());
        var startNonterminal = setup(factory) ?? throw new GrammarException("Setup callback did not return a valid start symbol");
        return new Parser(startNonterminal);
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

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCheckpoint = state.Input.Checkpoint();

            var results = _engine.Parse(state);

            startCheckpoint.Rewind();
            return new MultiResult<TOutput>(this, startCheckpoint, results.Alternatives, new ResultData(results.Statistics));
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state);

        public IEnumerable<IParser> GetChildren()
        {
            var visitor = new ChildParserListVisitor();
            return visitor.Visit(_startSymbol);
        }

        public override string ToString() => DefaultStringifier.ToString("Earley", Name, Id);

        public string GetBnf(BnfStringifyState state)
        {
            var grammarVisitor = new BnfGrammarVisitor();
            var bnf = grammarVisitor.Visit(_startSymbol, state);
            return $"EARLEY(\n{bnf}\n)";
        }

        public INamed SetName(string name) => new Parser(_startSymbol, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IEarleyPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
