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

    public sealed class Parser : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _inner;
        private readonly Func<IResult<TMiddle>, IParser<TInput, TOutput>> _getParser;
        private readonly IReadOnlyList<IParser> _mentions;

        public Parser(IParser<TInput, TMiddle> inner, Func<IResult<TMiddle>, IParser<TInput, TOutput>> getParser, IEnumerable<IParser> mentions, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));

            _inner = inner;
            _getParser = getParser;
            _mentions = mentions.OrEmptyIfNull().ToList();
            Name = name;
        }

        public Parser(IParser<TInput, TMiddle> inner, Action<IConfiguration> setup, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(setup, nameof(setup));

            var config = new Configuration();
            setup(config);
            _inner = inner;
            _getParser = r => config.Pick(r.Value);
            _mentions = config.GetChildren().ToList();
            Name = name;
        }

        public string Name { get; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = _inner.Parse(state);

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
                var nextParser = _getParser(initial);
                return nextParser ?? new FailParser<TInput, TOutput>("Get parser callback returned null");
            }
            catch
            {
                checkpoint.Rewind();
                throw;
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner }.Concat(_mentions);

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => new Parser(_inner, _getParser, _mentions, name);
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

    public sealed class Parser : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput> _inner;
        private readonly Func<IResult, IParser<TInput, TOutput>> _getParser;
        private readonly IReadOnlyList<IParser> _mentions;

        public Parser(IParser<TInput> inner, Func<IResult, IParser<TInput, TOutput>> getParser, IEnumerable<IParser> mentions, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));

            _inner = inner;
            _getParser = getParser;
            _mentions = mentions.OrEmptyIfNull().ToList();
            Name = name;
        }

        public Parser(IParser<TInput> inner, Action<IConfiguration> setup, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(setup, nameof(setup));

            var config = new Configuration();
            setup(config);
            _inner = inner;
            _getParser = r => config.Pick(r.Value);
            _mentions = config.GetChildren().ToList();
            Name = name;
        }

        public string Name { get; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = _inner.Parse(state);

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
                var nextParser = _getParser(initial);
                return nextParser ?? new FailParser<TInput, TOutput>("Get parser callback returned null");
            }
            catch
            {
                checkpoint.Rewind();
                throw;
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner }.Concat(_mentions);

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => new Parser(_inner, _getParser, _mentions, name);
    }
}
