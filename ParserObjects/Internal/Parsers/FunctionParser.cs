using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Invokes a delegate to perform the parse or match. The delegate may perform any logic necessary
/// and imposes no particular structure.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Function<TInput, TOutput>
{
    public delegate Result<TOutput> ParseFunc<TData>(IParseState<TInput> state, TData data, ResultFactory<TInput, TOutput> resultFactory);

    public delegate bool MatchFunc<TData>(IParseState<TInput> state, TData data);

    public static IParser<TInput, TOutput> Create<TData>(
        TData data,
        ParseFunc<TData> parseFunction,
        MatchFunc<TData>? matchFunction,
        string description,
        IReadOnlyList<IParser>? children,
        string name = ""
    ) => new Parser<TData>(data, parseFunction, matchFunction, description, children, name);

    public sealed class Parser<TData> : IParser<TInput, TOutput>
    {
        private readonly TData _data;
        private readonly ParseFunc<TData> _parseFunction;
        private readonly MatchFunc<TData> _matchFunction;
        private readonly IReadOnlyList<IParser> _children;

        public Parser(
            TData data,
            ParseFunc<TData> parseFunction,
            MatchFunc<TData>? matchFunction,
            string description,
            IReadOnlyList<IParser>? children,
            string name = ""
        )
        {
            _data = data;
            _parseFunction = parseFunction;
            _matchFunction = matchFunction ?? ((state, _) => Parse(state).Success);
            _children = children ?? [];
            Description = description;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public string Description { get; }

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);
            var startCheckpoint = state.Input.Checkpoint();

            var args = new ResultFactory<TInput, TOutput>(this, state, startCheckpoint);
            var result = _parseFunction(state, _data, args);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            return result with { Consumed = state.Input.Consumed - startCheckpoint.Consumed };
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);
            var startCheckpoint = state.Input.Checkpoint();

            var result = _matchFunction(state, _data);
            if (!result)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public IEnumerable<IParser> GetChildren() => _children;

        public override string ToString() => DefaultStringifier.ToString("Function (Single)", Name, Id);

        public INamed SetName(string name) => new Parser<TData>(_data, _parseFunction, _matchFunction, Description, _children, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public delegate MultiResult<TOutput> MultiParseFunc<TData>(IParseState<TInput> state, TData data, MultiResultBuilder resultBuilder);

    public static IMultiParser<TInput, TOutput> CreateMulti<TData>(
        TData data,
        MultiParseFunc<TData> parseFunction,
        string description,
        IReadOnlyList<IParser>? children,
        string name = ""
    ) => new MultiParser<TData>(data, parseFunction, description, children, name);

    public sealed class MultiParser<TData> : IMultiParser<TInput, TOutput>
    {
        private readonly TData _data;
        private readonly MultiParseFunc<TData> _parseFunction;
        private readonly IReadOnlyList<IParser> _children;

        public MultiParser(
            TData data,
            MultiParseFunc<TData> parseFunction,
            string description,
            IReadOnlyList<IParser>? children,
            string name = ""
        )
        {
            _data = data;
            _parseFunction = parseFunction;
            Description = description;
            _children = children ?? [];
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Description { get; }

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => _children;

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);
            var builder = new MultiResultBuilder(this, state, new List<ResultAlternative<TOutput>>(), state.Input.Checkpoint());
            return _parseFunction(state, _data, builder);
        }

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public override string ToString() => DefaultStringifier.ToString("Function (Multi)", Name, Id);

        public INamed SetName(string name) => new MultiParser<TData>(_data, _parseFunction, Description, _children, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public readonly struct MultiResultBuilder
    {
        private readonly IParser _parser;
        private readonly IParseState<TInput> _state;
        private readonly List<ResultAlternative<TOutput>> _results;
        private readonly SequenceCheckpoint _startCheckpoint;

        public MultiResultBuilder(IParser parser, IParseState<TInput> state, List<ResultAlternative<TOutput>> Results, SequenceCheckpoint StartCheckpoint)
        {
            _parser = parser;
            _state = state;
            _results = Results;
            _startCheckpoint = StartCheckpoint;
        }

        public MultiResultBuilder AddSuccesses(IEnumerable<TOutput> values)
        {
            var checkpoint = _state.Input.Checkpoint();
            var consumed = checkpoint.Consumed - _startCheckpoint.Consumed;
            foreach (var value in values)
                _results.Add(ResultAlternative<TOutput>.Ok(value, consumed, checkpoint));
            return this;
        }

        public MultiResult<TOutput> BuildResult()
            => new MultiResult<TOutput>(_parser, _results);
    }
}
