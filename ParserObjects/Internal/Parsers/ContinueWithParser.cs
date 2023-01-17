using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser to continue a multi-parse by feeding each successful result into a new parser in
/// series.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class ContinueWith<TInput, TMiddle, TOutput>
{
    public sealed class SingleParser : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly LeftValue _left;
        private readonly IParser<TInput, TOutput> _right;
        private readonly GetParserFromParser<TInput, TMiddle, TOutput> _getParser;

        public SingleParser(IMultiParser<TInput, TMiddle> inner, GetParserFromParser<TInput, TMiddle, TOutput> getParser, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _inner = inner;
            _left = new LeftValue(name);
            _right = getParser(_left);
            _getParser = getParser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner, _right };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multiResult = _inner.Parse(state);

            var results = new List<IResultAlternative<TOutput>>();

            foreach (var alt in multiResult.Results)
            {
                if (!alt.Success)
                    continue;

                alt.Continuation.Rewind();
                _left.Value = alt.Value;
                var result = _right.Parse(state);
                if (!result.Success)
                {
                    results.Add(new FailureResultAlternative<TOutput>(result.ErrorMessage, multiResult.StartCheckpoint));
                    continue;
                }

                results.Add(new SuccessResultAlternative<TOutput>(result.Value, result.Consumed, state.Input.Checkpoint()));
            }

            multiResult.StartCheckpoint.Rewind();
            return new MultiResult<TOutput>(this, multiResult.StartCheckpoint, results);
        }

        public override string ToString() => DefaultStringifier.ToString("ContinueWith", Name, Id);

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state);

        public INamed SetName(string name) => new SingleParser(_inner, _getParser, name);
    }

    public sealed class MultiParser : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly LeftValue _left;
        private readonly IMultiParser<TInput, TOutput> _right;
        private readonly GetMultiParserFromParser<TInput, TMiddle, TOutput> _getParser;

        public MultiParser(IMultiParser<TInput, TMiddle> inner, GetMultiParserFromParser<TInput, TMiddle, TOutput> getParser, string name = "")
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _inner = inner;
            _left = new LeftValue(name);
            _right = getParser(_left);
            _getParser = getParser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner, _right };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multiResult = _inner.Parse(state);

            var results = new List<IResultAlternative<TOutput>>();

            foreach (var alt in multiResult.Results)
            {
                if (!alt.Success)
                    continue;

                alt.Continuation.Rewind();
                _left.Value = alt.Value;
                var result = _right.Parse(state);
                if (!result.Success)
                {
                    results.Add(new FailureResultAlternative<TOutput>("Right parser returned no valid results", multiResult.StartCheckpoint));
                    continue;
                }

                foreach (var resultAlt in result.Results)
                    results.Add(new SuccessResultAlternative<TOutput>(resultAlt.Value, resultAlt.Consumed, resultAlt.Continuation));
            }

            multiResult.StartCheckpoint.Rewind();
            return new MultiResult<TOutput>(this, multiResult.StartCheckpoint, results);
        }

        public override string ToString() => DefaultStringifier.ToString("ContinueWith", Name, Id);

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state);

        public INamed SetName(string name) => new MultiParser(_inner, _getParser, name);
    }

    private class LeftValue : IParser<TInput, TMiddle>, IHiddenInternalParser
    {
        public LeftValue(string name)
        {
            Name = name;
        }

        public TMiddle? Value { get; set; }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IResult<TMiddle> Parse(IParseState<TInput> state) => state.Success(this, Value!, 0);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0);

        public bool Match(IParseState<TInput> state) => true;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename inner value parser");
    }
}
