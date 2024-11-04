using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static IParser<TInput, TOutput> Create<TData, TMiddle>(
        IParser<TInput, TMiddle> inner,
        TData data,
        GetOuter<TData, TMiddle> getOuter,
        IReadOnlyList<IParser> mentions,
        string name = ""
    ) => new Parser<TMiddle, TData>(inner, data, getOuter, mentions, name);

    public delegate IParser<TInput, TOutput> GetOuter<TData, TMiddle>(TData data, Result<TMiddle> result);

    public sealed class Parser<TMiddle, TData> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _inner;
        private readonly TData _data;
        private readonly GetOuter<TData, TMiddle> _getOuter;
        private readonly IReadOnlyList<IParser> _mentions;

        public Parser(IParser<TInput, TMiddle> inner, TData data, GetOuter<TData, TMiddle> getOuter, IReadOnlyList<IParser> mentions, string name = "")
        {
            _inner = inner;
            _data = data;
            _getOuter = getOuter;
            _mentions = mentions;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            var checkpoint = state.Input.Checkpoint();
            var initial = _inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Parse(state);
            if (nextResult.Success)
                return state.Success(nextParser, nextResult.Value, initial.Consumed + nextResult.Consumed);

            checkpoint.Rewind();
            return state.Fail(nextParser, nextResult.ErrorMessage);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            var checkpoint = state.Input.Checkpoint();
            var initial = _inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Match(state);
            if (nextResult)
                return true;

            checkpoint.Rewind();
            return false;
        }

        public IEnumerable<IParser> GetChildren() => new[] { _inner }.Concat(_mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain", Name, Id);

        public INamed SetName(string name) => new Parser<TMiddle, TData>(_inner, _data, _getOuter, _mentions, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }

        private IParser<TInput, TOutput> GetNextParser(SequenceCheckpoint checkpoint, Result<TMiddle> initial)
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
}
