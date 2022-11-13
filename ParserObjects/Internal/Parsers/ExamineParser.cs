﻿using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Inserts a callback before and after the specified parser. Useful for debugging purposes
/// and to adjust the input/output of a parser. Contains parsers and related machinery.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Examine<TInput, TOutput>
{
    /// <summary>
    /// Examine parser for parsers which return typed output. Executes callbacks before and
    /// after the parse.
    /// </summary>
    public sealed record Parser(
        IParser<TInput, TOutput> Inner,
        Action<Context>? Before,
        Action<Context>? After,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();
            var startConsumed = state.Input.Consumed;
            Before?.Invoke(new Context(Inner, state, null));
            var result = Inner.Parse(state);
            After?.Invoke(new Context(Inner, state, result));
            var totalConsumed = state.Input.Consumed - startConsumed;

            // The callbacks have access to Input, so the user might consume data. Make sure
            // to report that if it happens.
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            return result.AdjustConsumed(totalConsumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new List<IParser> { Inner };

        public override string ToString() => DefaultStringifier.ToString("Examine", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    public sealed record MultiParser(
        IMultiParser<TInput, TOutput> Inner,
        Action<MultiContext>? Before,
        Action<MultiContext>? After,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var startCheckpoint = state.Input.Checkpoint();

            var beforeFirstConsumed = state.Input.Consumed;
            Before?.Invoke(new MultiContext(Inner, state, null));
            var afterFirstConsumed = state.Input.Consumed;

            var result = Inner.Parse(state);

            var beforeSecondConsumed = state.Input.Consumed;
            After?.Invoke(new MultiContext(Inner, state, result));
            var afterSecondConsumed = state.Input.Consumed;

            var totalConsumedInCallbacks = afterFirstConsumed - beforeFirstConsumed + (afterSecondConsumed - beforeSecondConsumed);
            totalConsumedInCallbacks = totalConsumedInCallbacks < 0 ? 0 : totalConsumedInCallbacks;

            // The callbacks have access to Input, so the user might consume data. Make sure
            // to handle that correct in failure and success cases.
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            if (totalConsumedInCallbacks == 0)
                return result;

            return result.Recreate((alt, factory) => factory(alt.Value, alt.Consumed + totalConsumedInCallbacks, alt.Continuation), parser: this, startCheckpoint: startCheckpoint);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new List<IParser> { Inner };

        public override string ToString() => DefaultStringifier.ToString("Examine", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    /// <summary>
    /// Context information available during an examination.
    /// </summary>
    public record struct Context(
        IParser<TInput, TOutput> Parser,
        IParseState<TInput> State,
        IResult<TOutput>? Result
    )
    {
        public IDataStore Data => State.Data;
        public ISequence<TInput> Input => State.Input;
    }

    public record struct MultiContext(
        IMultiParser<TInput, TOutput> Parser,
        IParseState<TInput> State,
        IMultiResult<TOutput>? Result
    )
    {
        public IDataStore Data => State.Data;
        public ISequence<TInput> Input => State.Input;
    }
}

/// <summary>
/// Inserts a callback before and after parser execution. Used for parsers with untyped output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Examine<TInput>
{
    /// <summary>
    /// The examine parser. Executes callbacks before and after the parser. Does not return
    /// typed output.
    /// </summary>
    public sealed class Parser : IParser<TInput>
    {
        private readonly IParser<TInput> _parser;
        private readonly Action<Context>? _before;
        private readonly Action<Context>? _after;

        public Parser(IParser<TInput> parser, Action<Context>? before, Action<Context>? after, string name = "")
        {
            _parser = parser;
            _before = before;
            _after = after;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IResult Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            _before?.Invoke(new Context(_parser, state, null));
            var result = _parser.Parse(state);
            _after?.Invoke(new Context(_parser, state, result));
            return result;
        }

        public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

        public override string ToString() => DefaultStringifier.ToString("Examine", Name, Id);

        public INamed SetName(string name) => new Parser(_parser, _before, _after, name);
    }

    /// <summary>
    /// The context object which holds information able to be examined.
    /// </summary>
    public record struct Context(
        IParser<TInput> Parser,
        IParseState<TInput> State,
        IResult? Result
    )
    {
        public IDataStore Data => State.Data;
        public ISequence<TInput> Input => State.Input;
    }
}