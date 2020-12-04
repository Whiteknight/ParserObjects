using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Multi
{
    public class ReduceParser<TInput, TMulti, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IMultiResult<TMulti>, IResult<TOutput>> _reduce;
        private readonly IMultiParser<TInput, TMulti> _parser;

        public ReduceParser(IMultiParser<TInput, TMulti> parser, Func<IMultiResult<TMulti>, IResult<TOutput>> reduce)
        {
            _reduce = reduce;
            _parser = parser;
            Name = "";
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multi = _parser.Parse(state);
            return _reduce(multi);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class SelectSingleResultParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IMultiResult<TOutput>, IOption<IMultiResultAlternative<TOutput>>> _selector;
        private readonly IMultiParser<TInput, TOutput> _parser;

        public SelectSingleResultParser(IMultiParser<TInput, TOutput> parser, Func<IMultiResult<TOutput>, IOption<IMultiResultAlternative<TOutput>>> selector)
        {
            _selector = selector;
            _parser = parser;
            Name = "";
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multi = _parser.Parse(state);
            var selected = _selector(multi);
            if (!selected.Success)
                return state.Fail(this, "No alternative selected");
            if (!selected.Value.Success)
                return state.Fail(this, selected.Value.ErrorMessage);

            var alt = selected.Value;
            alt.Continuation.Rewind();
            return multi.ToResult(alt);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class ContinueWithParser<TInput, TMiddle, TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly Func<IParser<TInput, TMiddle>, IParser<TInput, TOutput>> _getParser;

        public ContinueWithParser(IMultiParser<TInput, TMiddle> inner, Func<IParser<TInput, TMiddle>, IParser<TInput, TOutput>> getParser)
        {
            _inner = inner;
            _getParser = getParser;
            Name = string.Empty;
        }

        public string Name { get; set; }

        // TODO: Is there any reasonable way to get the right-hand parser?
        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multiResult = _inner.Parse(state);

            var results = new List<IMultiResultAlternative<TOutput>>();

            foreach (var alt in multiResult.Results.Where(r => r.Success))
            {
                var left = new LeftValue(alt.Value, multiResult.Location);
                var rightParser = _getParser(left);
                alt.Continuation.Rewind();
                var result = rightParser.Parse(state);
                if (!result.Success)
                {
                    results.Add(new FailureMultiResultAlternative<TOutput>(result.ErrorMessage, multiResult.StartCheckpoint));
                    continue;
                }

                results.Add(new SuccessMultiResultAlternative<TOutput>(result.Value, result.Consumed, state.Input.Checkpoint()));
            }

            multiResult.StartCheckpoint.Rewind();
            return new MultiResult<TOutput>(this, multiResult.Location, multiResult.StartCheckpoint, results);
        }

        public override string ToString() => DefaultStringifier.ToString(this);

        private class LeftValue : IParser<TInput, TMiddle>
        {
            public LeftValue(TMiddle value, Location location)
            {
                Name = "LEFT";
                Value = value;
                Location = location;
            }

            public TMiddle Value { get; }

            public Location Location { get; }

            public string Name { get; set; }

            public IResult<TMiddle> Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location!);

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location!);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
