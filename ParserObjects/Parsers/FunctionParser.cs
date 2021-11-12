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
        public delegate IResult<TOutput> FailureFactory(string error, Location? location = null);

        public delegate IResult<TOutput> SuccessFactory(TOutput value, Location? location = null);

        public delegate IResult<TOutput> ParserFunction(IParseState<TInput> t, SuccessFactory success, FailureFactory fail);

        public delegate void FailureAdder(string error);

        public delegate void SuccessAdder(TOutput value);

        public delegate void MultiParserBuilder(IParseState<TInput> t, SuccessAdder addSuccess, FailureAdder addFailure);

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Func<IParseState<TInput>, IResult<TOutput>> _func;
            private readonly IReadOnlyList<IParser> _children;

            public Parser(Func<IParseState<TInput>, IResult<TOutput>> func, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = func;
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            public Parser(ParserFunction func, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = state => AdaptParserFunctionToFunc(this, state, func);
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            private static IResult<TOutput> AdaptParserFunctionToFunc(IParser<TInput, TOutput> parser, IParseState<TInput> state, ParserFunction func)
            {
                var startConsumed = state.Input.Consumed;
                IResult<TOutput> OnSuccess(TOutput value, Location? loc)
                    => state.Success(parser, value, state.Input.Consumed - startConsumed, loc ?? state.Input.CurrentLocation);

                IResult<TOutput> OnFailure(string err, Location? loc)
                    => state.Fail(parser, err, loc ?? state.Input.CurrentLocation);

                return func(state, OnSuccess, OnFailure);
            }

            public string Name { get; set; }

            public string? Description { get; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startCheckpoint = state.Input.Checkpoint();

                var result = _func(state);
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

            public IEnumerable<IParser> GetChildren() => _children;

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        public class MultiParser : IMultiParser<TInput, TOutput>
        {
            private readonly Func<IParseState<TInput>, IMultiResult<TOutput>> _func;
            private readonly IReadOnlyList<IParser> _children;

            public MultiParser(MultiParserBuilder builder, string? description, IEnumerable<IParser>? children)
            {
                Assert.ArgumentNotNull(builder, nameof(builder));
                _func = (state) => AdaptMultiParserBuilderToFunction(this, builder, state);
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            public MultiParser(Func<IParseState<TInput>, IMultiResult<TOutput>> func, string? description, IEnumerable<IParser> children)
            {
                Assert.ArgumentNotNull(func, nameof(func));
                _func = func;
                Name = string.Empty;
                Description = description;
                var childList = children?.ToList() as IReadOnlyList<IParser>;
                _children = childList ?? Array.Empty<IParser>();
            }

            private static IMultiResult<TOutput> AdaptMultiParserBuilderToFunction(IParser parser, MultiParserBuilder builder, IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(builder, nameof(builder));
                var startCheckpoint = state.Input.Checkpoint();

                var results = new List<IResultAlternative<TOutput>>();

                void AddSuccess(TOutput value)
                {
                    var checkpoint = state.Input.Checkpoint();
                    var consumed = checkpoint.Consumed - startCheckpoint.Consumed;
                    results.Add(new SuccessResultAlternative<TOutput>(value, consumed, checkpoint));
                }

                void AddFailure(string err)
                {
                    results.Add(new FailureResultAlternative<TOutput>(err, startCheckpoint));
                }

                builder(state, AddSuccess, AddFailure);

                startCheckpoint.Rewind();
                return new MultiResult<TOutput>(parser, startCheckpoint.Location, startCheckpoint, results);
            }

            public string Name { get; set; }

            public string? Description { get; }

            public IEnumerable<IParser> GetChildren() => _children;

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                return _func(state);
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
