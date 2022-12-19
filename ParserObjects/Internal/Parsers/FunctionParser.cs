using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Invokes a delegate to perform the parse. The delegate may perform any logic necessary and
/// imposes no particular structure.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Function<TInput, TOutput>
{
    public record struct SingleArguments(IParser<TInput, TOutput> Parser, IParseState<TInput> State, SequenceCheckpoint StartCheckpoint)
    {
        public ISequence<TInput> Input => State.Input;
        public IDataStore Data => State.Data;
        public IResultsCache Cache => State.Cache;
        public IResult<TOutput> Failure(string errorMessage, Location? location = null, IReadOnlyList<object>? data = null)
            => State.Fail(Parser, errorMessage, location ?? State.Input.CurrentLocation, data);

        public IResult<TOutput> Success(TOutput value, Location? location = null, IReadOnlyList<object>? data = null)
            => State.Success(Parser, value, State.Input.Consumed - StartCheckpoint.Consumed, location ?? State.Input.CurrentLocation, data);
    }

    public sealed record Parser(
        Func<SingleArguments, IResult<TOutput>> Function,
        string Description,
        IReadOnlyList<IParser> Children,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            var args = new SingleArguments(this, state, startCheckpoint);
            var result = Function(args);
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

        // TODO: Consider taking a second Func() delegate to implement an optimized Match(),
        // where necessary.
        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();

            var args = new SingleArguments(this, state, startCheckpoint);
            var result = Function(args);
            if (result == null)
                return false;

            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public IEnumerable<IParser> GetChildren() => Children ?? Array.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString("Function (Single)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    public record struct MultiArguments(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State)
    {
        public ISequence<TInput> Input => State.Input;
        public IDataStore Data => State.Data;
        public IResultsCache Cache => State.Cache;
    }

    public record struct MultiBuilder(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State, IList<IResultAlternative<TOutput>> Results, SequenceCheckpoint StartCheckpoint)
    {
        public ISequence<TInput> Input => State.Input;
        public IDataStore Data => State.Data;
        public IResultsCache Cache => State.Cache;

        public void AddSuccess(TOutput value)
        {
            var checkpoint = Input.Checkpoint();
            var consumed = checkpoint.Consumed - StartCheckpoint.Consumed;
            Results.Add(new SuccessResultAlternative<TOutput>(value, consumed, checkpoint));
        }

        public void AddSuccesses(IEnumerable<TOutput> values)
        {
            var checkpoint = Input.Checkpoint();
            var consumed = checkpoint.Consumed - StartCheckpoint.Consumed;
            foreach (var value in values)
                Results.Add(new SuccessResultAlternative<TOutput>(value, consumed, checkpoint));
        }

        public void AddFailure(string err)
        {
            Results.Add(new FailureResultAlternative<TOutput>(err, StartCheckpoint));
        }
    }

    public sealed record MultiParser(
        Func<MultiArguments, IMultiResult<TOutput>> Function,
        string Description,
        IReadOnlyList<IParser> Children,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public MultiParser(Action<MultiBuilder> builder, string description, IReadOnlyList<IParser> children, string name = "")
            : this((args) => AdaptMultiParserBuilderToFunction(args, builder), description, children, name)
        {
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        private static IMultiResult<TOutput> AdaptMultiParserBuilderToFunction(MultiArguments args, Action<MultiBuilder> build)
        {
            Assert.ArgumentNotNull(build, nameof(build));
            var startCheckpoint = args.Input.Checkpoint();

            var buildArgs = new MultiBuilder(args.Parser, args.State, new List<IResultAlternative<TOutput>>(), startCheckpoint);
            build(buildArgs);

            startCheckpoint.Rewind();
            return new MultiResult<TOutput>(args.Parser, startCheckpoint.Location, startCheckpoint, buildArgs.Results);
        }

        public IEnumerable<IParser> GetChildren() => Children;

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var args = new MultiArguments(this, state);
            return Function(args);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString("Function (Multi)", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}

public static class Function<TInput>
{
    public sealed class Parser : IParser<TInput>
    {
        private readonly Func<IParseState<TInput>, IResult> _func;
        private readonly IReadOnlyList<IParser> _children;

        public Parser(Func<IParseState<TInput>, IResult> func, string? description, IEnumerable<IParser>? children, string name = "")
        {
            Assert.ArgumentNotNull(func, nameof(func));
            _func = func;
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
            var result = _func(state);
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
            var result = _func(state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public override string ToString() => DefaultStringifier.ToString("Function", Name, Id);

        public INamed SetName(string name) => new Parser(_func, Description, _children, name);
    }
}
