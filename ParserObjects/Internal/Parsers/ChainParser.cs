using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Executes the given parser and uses the value returned to select the next parser to execute.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Chain<TInput, TMiddle, TOutput>
{
    public static IParser<TInput, TOutput> Configure(IParser<TInput, TMiddle> inner, Action<ParserPredicateSelector<TInput, TMiddle, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        Assert.ArgumentNotNull(setup, nameof(setup));
        var config = new ParserPredicateSelector<TInput, TMiddle, TOutput>(new List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)>());
        setup(config);
        return new Parser<ParserPredicateSelector<TInput, TMiddle, TOutput>>(inner, config, static (c, r) => c.Pick(r.Value), config.GetChildren().ToList(), name);
    }

    public sealed record Parser<TData>(
        IParser<TInput, TMiddle> Inner,
        TData Data,
        Func<TData, IResult<TMiddle>, IParser<TInput, TOutput>> GetParser,
        IReadOnlyList<IParser> Mentions,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = Inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Parse(state);
            if (nextResult.Success)
                return state.Success(nextParser, nextResult.Value, initial.Consumed + nextResult.Consumed, nextResult.Location);

            checkpoint.Rewind();
            return state.Fail(nextParser, nextResult.ErrorMessage, nextResult.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = Inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Match(state);
            if (nextResult)
                return true;

            checkpoint.Rewind();
            return false;
        }

        public IEnumerable<IParser> GetChildren() => new[] { Inner }.Concat(Mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain (Single)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        private IParser<TInput, TOutput> GetNextParser(SequenceCheckpoint checkpoint, IResult<TMiddle> initial)
        {
            try
            {
                var nextParser = GetParser(Data, initial);
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

public static class Chain<TInput, TOutput>
{
    public static IParser<TInput, TOutput> Configure(IParser<TInput> inner, Action<ParserPredicateSelector<TInput, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        Assert.ArgumentNotNull(setup, nameof(setup));

        var config = new ParserPredicateSelector<TInput, TOutput>(new List<(Func<object, bool> equals, IParser<TInput, TOutput> parser)>());
        setup(config);
        return new Parser<ParserPredicateSelector<TInput, TOutput>>(inner, config, static (c, r) => c.Pick(r.Value), config.GetChildren().ToList(), name);
    }

    public sealed record Parser<TData>(
        IParser<TInput> Inner,
        TData Data,
        Func<TData, IResult, IParser<TInput, TOutput>> GetParser,
        IReadOnlyList<IParser> Mentions,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = Inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Parse(state);
            if (nextResult.Success)
                return state.Success(nextParser, nextResult.Value, initial.Consumed + nextResult.Consumed, nextResult.Location);

            checkpoint.Rewind();
            return state.Fail(nextParser, nextResult.ErrorMessage, nextResult.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = Inner.Parse(state);

            var nextParser = GetNextParser(checkpoint, initial);
            var nextResult = nextParser.Match(state);
            if (nextResult)
                return true;

            checkpoint.Rewind();
            return false;
        }

        public IEnumerable<IParser> GetChildren() => new[] { Inner }.Concat(Mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain (Multi)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        private IParser<TInput, TOutput> GetNextParser(SequenceCheckpoint checkpoint, IResult initial)
        {
            try
            {
                var nextParser = GetParser(Data, initial);
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
