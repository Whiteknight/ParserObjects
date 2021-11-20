using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a delegate to perform the parse. The delegate may perform any logic necessary and
    /// imposes no particular structure.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Function<TInput, TOutput>
    {
        public record struct SingleArguments(IParser<TInput, TOutput> Parser, IParseState<TInput> State, ISequenceCheckpoint StartCheckpoint)
        {
            public ISequence<TInput> Input => State.Input;
            public IDataStore Data => State.Data;
            public IResultsCache Cache => State.Cache;
            public IResult<TOutput> Failure(string errorMessage, Location? location = null)
                => State.Fail(Parser, errorMessage, location ?? State.Input.CurrentLocation);

            public IResult<TOutput> Success(TOutput value, Location? location = null)
                => State.Success(Parser, value, State.Input.Consumed - StartCheckpoint.Consumed, location ?? State.Input.CurrentLocation);
        }

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Func<SingleArguments, IResult<TOutput>> _func;
            private readonly IReadOnlyList<IParser>? _children;

            public Parser(Func<SingleArguments, IResult<TOutput>> func, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = func;
                Name = string.Empty;
                Description = description;
                _children = children?.ToList();
            }

            public string Name { get; set; }

            public string? Description { get; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startCheckpoint = state.Input.Checkpoint();

                var args = new SingleArguments(this, state, startCheckpoint);
                var result = _func(args);
                if (result == null)
                    return state.Fail(this, "No result returned");

                if (!result.Success)
                {
                    startCheckpoint.Rewind();
                    if (result.Consumed != 0)
                        return state.Fail(this, result.ErrorMessage);
                    return result;
                }

                var totalConsumed = state.Input.Consumed - startCheckpoint.Consumed;
                if (result.Consumed != totalConsumed)
                    return state.Success(this, result.Value, totalConsumed);
                return result;
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => _children ?? Array.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        public record struct MultiArguments(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State)
        {
            public ISequence<TInput> Input => State.Input;
            public IDataStore Data => State.Data;
            public IResultsCache Cache => State.Cache;
        }

        public record struct MultiBuilder(IMultiParser<TInput, TOutput> Parser, IParseState<TInput> State, IList<IResultAlternative<TOutput>> Results, ISequenceCheckpoint StartCheckpoint)
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

        public class MultiParser : IMultiParser<TInput, TOutput>
        {
            private readonly Func<MultiArguments, IMultiResult<TOutput>> _func;
            private readonly IReadOnlyList<IParser> _children;

            public MultiParser(Action<MultiBuilder> builder, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(builder, nameof(builder));
                _func = args => AdaptMultiParserBuilderToFunction(args, builder);
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            public MultiParser(Func<MultiArguments, IMultiResult<TOutput>> func, string? description, IEnumerable<IParser> children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = func;
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            private static IMultiResult<TOutput> AdaptMultiParserBuilderToFunction(MultiArguments args, Action<MultiBuilder> build)
            {
                Assert.ArgumentNotNull(build, nameof(build));
                var startCheckpoint = args.Input.Checkpoint();

                var buildArgs = new MultiBuilder(args.Parser, args.State, new List<IResultAlternative<TOutput>>(), startCheckpoint);
                build(buildArgs);

                startCheckpoint.Rewind();
                return new MultiResult<TOutput>(args.Parser, startCheckpoint.Location, startCheckpoint, buildArgs.Results);
            }

            public string Name { get; set; }

            public string? Description { get; }

            public IEnumerable<IParser> GetChildren() => _children;

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var args = new MultiArguments(this, state);
                return _func(args);
            }

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }

    public static class Function<TInput>
    {
        public class Parser : IParser<TInput>
        {
            private readonly Func<IParseState<TInput>, IResult> _func;
            private readonly IReadOnlyList<IParser> _children;

            public Parser(Func<IParseState<TInput>, IResult> func, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = func;
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            public string Name { get; set; }

            public string? Description { get; }

            public IEnumerable<IParser> GetChildren() => _children;

            public IResult Parse(IParseState<TInput> state)
            {
                var startCheckpoint = state.Input.Checkpoint();
                var matched = _func(state);
                if (!matched.Success)
                {
                    startCheckpoint.Rewind();
                    return state.Fail(this, "Matcher returned false");
                }

                return state.Success(this, Defaults.ObjectInstance, state.Input.Consumed - startCheckpoint.Consumed);
            }

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
