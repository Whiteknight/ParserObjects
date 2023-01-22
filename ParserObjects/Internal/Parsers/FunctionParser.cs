using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;
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
    public delegate IResult<TOutput> ParseFunc<TData>(IParseState<TInput> state, TData data, ResultFactory<TInput, TOutput> resultFactory);

    public delegate bool MatchFunc<TData>(IParseState<TInput> state, TData data);

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
            _matchFunction = matchFunction ?? ((state, _) => Parse(state)?.Success ?? false);
            _children = children ?? Array.Empty<IParser>();
            Description = description;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public string Description { get; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            var args = new ResultFactory<TInput, TOutput>(this, state, startCheckpoint);
            var result = _parseFunction(state, _data, args);
            if (result == null)
                return state.Fail(this, "No result returned");

            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            var totalConsumed = state.Input.Consumed - startCheckpoint.Consumed;
            return result.AdjustConsumed(totalConsumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            var result = _matchFunction(state, _data);

            if (!result)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public IEnumerable<IParser> GetChildren() => _children ?? Array.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString("Function (Single)", Name, Id);

        public INamed SetName(string name) => new Parser<TData>(_data, _parseFunction, _matchFunction, Description, _children, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public record struct MultiArguments(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State)
    {
        public ISequence<TInput> Input => State.Input;
        public DataStore Data => State.Data;
        public IResultsCache Cache => State.Cache;
    }

    public record struct MultiBuilder(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State, IList<IResultAlternative<TOutput>> Results, SequenceCheckpoint StartCheckpoint)
    {
        public ISequence<TInput> Input => State.Input;

        public void AddSuccesses(IEnumerable<TOutput> values)
        {
            var checkpoint = Input.Checkpoint();
            var consumed = checkpoint.Consumed - StartCheckpoint.Consumed;
            foreach (var value in values)
                Results.Add(new SuccessResultAlternative<TOutput>(value, consumed, checkpoint));
        }
    }

    public sealed class MultiParser<TData> : IMultiParser<TInput, TOutput>
    {
        private readonly TData _data;
        private readonly Func<TData, Function<TInput, TOutput>.MultiArguments, IMultiResult<TOutput>> _parseFunction;
        private readonly IReadOnlyList<IParser> _children;

        public MultiParser(
            TData data,
            Func<TData, MultiArguments, IMultiResult<TOutput>> parseFunction,
            string description,
            IReadOnlyList<IParser>? children,
            string name = ""
        )
        {
            _data = data;
            _parseFunction = parseFunction;
            Description = description;
            _children = children ?? Array.Empty<IParser>();
            Name = name;
        }

        public MultiParser(TData data, Action<TData, MultiBuilder> builder, string description, IReadOnlyList<IParser> children, string name = "")
            : this(data, (d, args) => AdaptMultiParserBuilderToFunction(d, args, builder), description, children, name)
        {
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();
        public string Description { get; }
        public string Name { get; }

        private static IMultiResult<TOutput> AdaptMultiParserBuilderToFunction(TData data, MultiArguments args, Action<TData, MultiBuilder> build)
        {
            Assert.ArgumentNotNull(build, nameof(build));
            var startCheckpoint = args.Input.Checkpoint();

            var buildArgs = new MultiBuilder(args.Parser, args.State, new List<IResultAlternative<TOutput>>(), startCheckpoint);
            build(data, buildArgs);

            startCheckpoint.Rewind();
            return new MultiResult<TOutput>(args.Parser, startCheckpoint, buildArgs.Results);
        }

        public IEnumerable<IParser> GetChildren() => _children;

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var args = new MultiArguments(this, state);
            return _parseFunction(_data, args);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString("Function (Multi)", Name, Id);

        public INamed SetName(string name) => new MultiParser<TData>(_data, _parseFunction, Description, _children, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}

public static class Function<TInput>
{
    public sealed class Parser<TData> : IParser<TInput>
    {
        private readonly TData _data;
        private readonly Func<TData, IParseState<TInput>, IResult> _parseFunction;
        private readonly Func<TData, IParseState<TInput>, bool> _matchFunction;
        private readonly IReadOnlyList<IParser> _children;

        public Parser(
            TData data,
            Func<TData, IParseState<TInput>, IResult> parseFunction,
            Func<TData, IParseState<TInput>, bool>? matchFunction,
            string? description,
            IEnumerable<IParser>? children,
            string name = ""
        )
        {
            Assert.ArgumentNotNull(parseFunction, nameof(parseFunction));
            _data = data;
            _parseFunction = parseFunction;
            _matchFunction = matchFunction ?? ((_, state) => Parse(state)?.Success ?? false);
            Name = name;
            Description = description;
            var childList = children?.ToArray() as IReadOnlyList<IParser>;
            _children = childList ?? Array.Empty<IParser>();
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public string? Description { get; }

        public IEnumerable<IParser> GetChildren() => _children;

        public IResult Parse(IParseState<TInput> state)
        {
            var startCheckpoint = state.Input.Checkpoint();
            var result = _parseFunction(_data, state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }

            var totalConsumed = state.Input.Consumed - startCheckpoint.Consumed;
            return result.AdjustConsumed(totalConsumed);
        }

        public bool Match(IParseState<TInput> state)
        {
            var startCheckpoint = state.Input.Checkpoint();
            var result = _parseFunction(_data, state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public override string ToString() => DefaultStringifier.ToString("Function", Name, Id);

        public INamed SetName(string name) => new Parser<TData>(_data, _parseFunction, _matchFunction, Description, _children, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
