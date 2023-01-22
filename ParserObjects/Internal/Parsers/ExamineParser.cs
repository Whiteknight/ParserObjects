using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;
using ParserObjects.Internal.Visitors;

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
        Action<ParseContext<TInput, TOutput>>? Before,
        Action<ParseContext<TInput, TOutput>>? After,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();
            var startConsumed = state.Input.Consumed;
            Before?.Invoke(new ParseContext<TInput, TOutput>(Inner, state, null));
            var result = Inner.Parse(state);
            After?.Invoke(new ParseContext<TInput, TOutput>(Inner, state, result));
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

        public bool Match(IParseState<TInput> state) => Parse(state).Success;

        public IEnumerable<IParser> GetChildren() => new List<IParser> { Inner };

        public override string ToString() => DefaultStringifier.ToString("Examine", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record MultiParser(
        IMultiParser<TInput, TOutput> Inner,
        Action<MultiParseContext<TInput, TOutput>>? Before,
        Action<MultiParseContext<TInput, TOutput>>? After,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var startCheckpoint = state.Input.Checkpoint();

            var beforeFirstConsumed = state.Input.Consumed;
            Before?.Invoke(new MultiParseContext<TInput, TOutput>(Inner, state, null));
            var afterFirstConsumed = state.Input.Consumed;

            var result = Inner.Parse(state);

            var beforeSecondConsumed = state.Input.Consumed;
            After?.Invoke(new MultiParseContext<TInput, TOutput>(Inner, state, result));
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

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}

/// <summary>
/// Inserts a callback before and after parser execution. Used for parsers with untyped output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class ExamineParser<TInput> : IParser<TInput>
{
    private readonly IParser<TInput> _parser;
    private readonly Action<ParseContext<TInput>>? _before;
    private readonly Action<ParseContext<TInput>>? _after;

    public ExamineParser(IParser<TInput> parser, Action<ParseContext<TInput>>? before, Action<ParseContext<TInput>>? after, string name = "")
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
        _before?.Invoke(new ParseContext<TInput>(_parser, state, null));
        var result = _parser.Parse(state);
        _after?.Invoke(new ParseContext<TInput>(_parser, state, result));
        return result;
    }

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

    public override string ToString() => DefaultStringifier.ToString("Examine", Name, Id);

    public INamed SetName(string name) => new ExamineParser<TInput>(_parser, _before, _after, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }
}
