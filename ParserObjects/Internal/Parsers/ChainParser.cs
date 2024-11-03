using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Executes the given parser and uses the value returned to select the next parser to execute.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Chain<TInput, TOutput>
{
    /* The Chain parser executes a callback to get the next parser after the initial parser has
     * completed. In simple cases this is a user-defined callback and imposes very little structure
     * on what is returned or how.
     *
     * The ParserPredicateSelector struct and the Configure() methods give us the Choose() behavior
     * where we have a fluent interface to specify results. Internally that ParserPredicateSelector
     * is just wrapped in a callback and passed to the Chain.Parser like normal
     */

    public static IParser<TInput, TOutput> Configure<TMiddle>(IParser<TInput, TMiddle> inner, Action<ParserPredicateSelector<TInput, TMiddle, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner);
        Assert.ArgumentNotNull(setup);
        var config = new ParserPredicateSelector<TInput, TMiddle, TOutput>(new List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)>());
        setup(config);
        return new Parser<TMiddle, ParserPredicateSelector<TInput, TMiddle, TOutput>>(inner, config, static (c, r) => c.Pick(r.Value), config.GetChildren().ToList(), name);
    }

    private readonly struct InternalParser<TInnerParser, TInnerResult, TData>
        where TInnerParser : IParser<TInput>
    {
        private readonly TData _data;
        private readonly Func<TInnerParser, IParseState<TInput>, Result<TInnerResult>> _getResult;
        private readonly Func<TData, Result<TInnerResult>, IParser<TInput, TOutput>> _getOuter;

        public TInnerParser Inner { get; }

        public InternalParser(
            TInnerParser inner,
            TData data,
            Func<TInnerParser, IParseState<TInput>, Result<TInnerResult>> getResult,
            Func<TData, Result<TInnerResult>, IParser<TInput, TOutput>> getOuter)
        {
            Inner = inner;
            _data = data;
            _getResult = getResult;
            _getOuter = getOuter;
        }

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            var checkpoint = state.Input.Checkpoint();
            var initial = _getResult(Inner, state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Parse(state);
            if (nextResult.Success)
                return state.Success(nextParser, nextResult.Value, initial.Consumed + nextResult.Consumed);

            checkpoint.Rewind();
            return state.Fail(nextParser, nextResult.ErrorMessage);
        }

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            var checkpoint = state.Input.Checkpoint();
            var initial = _getResult(Inner, state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Match(state);
            if (nextResult)
                return true;

            checkpoint.Rewind();
            return false;
        }

        private IParser<TInput, TOutput> GetNextParser(SequenceCheckpoint checkpoint, Result<TInnerResult> initial)
        {
            try
            {
                var nextParser = _getOuter(_data, initial);
                return nextParser ?? new FailParser<TInput, TOutput>("Get parser callback returned null");
            }
            catch
            {
                checkpoint.Rewind();
                throw;
            }
        }
    }

    public sealed class Parser<TMiddle, TData> : IParser<TInput, TOutput>
    {
        private readonly InternalParser<IParser<TInput, TMiddle>, TMiddle, TData> _internal;
        private readonly IReadOnlyList<IParser> _mentions;

        public Parser(IParser<TInput, TMiddle> inner, TData data, Func<TData, Result<TMiddle>, IParser<TInput, TOutput>> getOuter, IReadOnlyList<IParser> mentions, string name = "")
        {
            _internal = new InternalParser<IParser<TInput, TMiddle>, TMiddle, TData>(inner, data, static (p, s) => p.Parse(s), getOuter);
            _mentions = mentions;
            Name = name;
        }

        private Parser(InternalParser<IParser<TInput, TMiddle>, TMiddle, TData> internalData, IReadOnlyList<IParser> mentions, string name)
        {
            _internal = internalData;
            _mentions = mentions;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public Result<TOutput> Parse(IParseState<TInput> state) => _internal.Parse(state);

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => _internal.Parse(state).AsObject();

        public bool Match(IParseState<TInput> state) => _internal.Match(state);

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Inner }.Concat(_mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain (Single)", Name, Id);

        public INamed SetName(string name) => new Parser<TMiddle, TData>(_internal, _mentions, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
