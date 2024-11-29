using System;
using System.Collections.Generic;
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

    public static IParser<TInput, TOutput> Configure<TMiddle>(IParser<TInput, TMiddle> inner, Action<ParserPredicateBuilder<TInput, TMiddle, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner);
        Assert.ArgumentNotNull(setup);
        var config = new ParserPredicateBuilder<TInput, TMiddle, TOutput>([]);
        setup(config);
        var selector = new Selector<TMiddle>(config.Parsers);
        return new Parser<TMiddle, Selector<TMiddle>>(inner, selector, static (c, r) => c.Pick(r.Value), [.. selector.GetChildren()], name);
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
                return Result.Ok(nextParser, nextResult.Value, initial.Consumed + nextResult.Consumed);

            checkpoint.Rewind();
            return Result.Fail(nextParser, nextResult.ErrorMessage);
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

        public IEnumerable<IParser> GetChildren() => [_inner, .. _mentions];

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

    public readonly record struct Selector<TMiddle>(
        IReadOnlyList<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> Parsers
    )
    {
        /// <summary>
        /// Return the first parser whose predicate matches the given next item.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public IParser<TInput, TOutput> Pick(TMiddle next)
        {
            foreach (var (equals, parser) in Parsers)
            {
                if (equals(next))
                    return parser;
            }

            return Parsers<TInput>.Fail<TOutput>($"No configured parsers handle {next}");
        }

        /// <summary>
        /// Return a list of all parsers held in this object.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<IParser> GetChildren()
        {
            var children = new IParser[Parsers.Count];
            for (int i = 0; i < Parsers.Count; i++)
                children[i] = Parsers[i].parser;
            return children;
        }
    }
}
