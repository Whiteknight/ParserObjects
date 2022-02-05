using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Executes the given parser and uses the value returned to select the next parser to execute.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Chain<TInput, TMiddle, TOutput>
{
    /// <summary>
    /// Configures the parsers which may be selected and invoked by the Chain parser.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Specify a predicate and the parser to invoke if the predicate is true.
        /// </summary>
        /// <param name="equals"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        IConfiguration When(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser);
    }

    private sealed class Configuration : IConfiguration
    {
        private readonly List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

        public Configuration()
        {
            _parsers = new List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)>();
        }

        public IConfiguration When(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)
        {
            Assert.ArgumentNotNull(equals, nameof(equals));
            Assert.ArgumentNotNull(parser, nameof(parser));
            _parsers.Add((equals, parser));
            return this;
        }

        public IParser<TInput, TOutput> Pick(TMiddle next)
            => _parsers
                .Where(rule => rule.equals(next))
                .Select(rule => rule.parser)
                .FirstOrDefault() ?? new FailParser<TInput, TOutput>($"No configured parsers handle {next}");

        public IEnumerable<IParser> GetChildren()
            => _parsers.Select(v => v.parser);
    }

    public static Parser Configure(IParser<TInput, TMiddle> inner, Action<IConfiguration> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        Assert.ArgumentNotNull(setup, nameof(setup));
        var config = new Configuration();
        setup(config);
        return new Parser(inner, r => config.Pick(r.Value), config.GetChildren().ToList(), name);
    }

    public sealed record Parser(
        IParser<TInput, TMiddle> Inner,
        Func<IResult<TMiddle>, IParser<TInput, TOutput>> GetParser,
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

        private IParser<TInput, TOutput> GetNextParser(ISequenceCheckpoint checkpoint, IResult<TMiddle> initial)
        {
            try
            {
                var nextParser = GetParser(initial);
                return nextParser ?? new FailParser<TInput, TOutput>("Get parser callback returned null");
            }
            catch
            {
                checkpoint.Rewind();
                throw;
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner }.Concat(Mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain (Single)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}

public static class Chain<TInput, TOutput>
{
    /// <summary>
    /// Configures the parsers to be invoked by the Chain parser.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Specify a predicate and the parser to invoke if the predicate returns true.
        /// </summary>
        /// <param name="equals"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        IConfiguration When(Func<object, bool> equals, IParser<TInput, TOutput> parser);
    }

    private sealed class Configuration : IConfiguration
    {
        private readonly List<(Func<object, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

        public Configuration()
        {
            _parsers = new List<(Func<object, bool> equals, IParser<TInput, TOutput> parser)>();
        }

        public IConfiguration When(Func<object, bool> equals, IParser<TInput, TOutput> parser)
        {
            Assert.ArgumentNotNull(equals, nameof(equals));
            Assert.ArgumentNotNull(parser, nameof(parser));
            _parsers.Add((equals, parser));
            return this;
        }

        public IParser<TInput, TOutput> Pick(object next)
            => _parsers
                .Where(rule => rule.equals(next))
                .Select(rule => rule.parser)
                .FirstOrDefault() ?? new FailParser<TInput, TOutput>($"No configured parsers handle {next}");

        public IEnumerable<IParser> GetChildren()
            => _parsers.Select(v => v.parser);
    }

    public static Parser Configure(IParser<TInput> inner, Action<IConfiguration> setup, string name = "")
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        Assert.ArgumentNotNull(setup, nameof(setup));

        var config = new Configuration();
        setup(config);
        return new Parser(inner, r => config.Pick(r.Value), config.GetChildren().ToList(), name);
    }

    public sealed record Parser(
        IParser<TInput> Inner,
        Func<IResult, IParser<TInput, TOutput>> GetParser,
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

        private IParser<TInput, TOutput> GetNextParser(ISequenceCheckpoint checkpoint, IResult initial)
        {
            try
            {
                var nextParser = GetParser(initial);
                return nextParser ?? new FailParser<TInput, TOutput>("Get parser callback returned null");
            }
            catch
            {
                checkpoint.Rewind();
                throw;
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner }.Concat(Mentions);

        public override string ToString() => DefaultStringifier.ToString("Chain (Multi)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}
