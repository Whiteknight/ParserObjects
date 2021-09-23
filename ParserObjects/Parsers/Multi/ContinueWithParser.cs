using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Multi
{
    public static class ContinueWith
    {
        public delegate IParser<TInput, TOutput> SingleParserSelector<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

        public delegate IMultiParser<TInput, TOutput> MultiParserSelector<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

        public class SingleParser<TInput, TMiddle, TOutput> : IMultiParser<TInput, TOutput>
        {
            private readonly IMultiParser<TInput, TMiddle> _inner;
            private readonly SingleParserSelector<TInput, TMiddle, TOutput> _getParser;

            public SingleParser(IMultiParser<TInput, TMiddle> inner, SingleParserSelector<TInput, TMiddle, TOutput> getParser)
            {
                _inner = inner;
                _getParser = getParser;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                var multiResult = _inner.Parse(state);

                var results = new List<IResultAlternative<TOutput>>();

                foreach (var alt in multiResult.Results.Where(r => r.Success))
                {
                    var left = new LeftValue(alt.Value, multiResult.Location);
                    var rightParser = _getParser(left);
                    alt.Continuation.Rewind();
                    var result = rightParser.Parse(state);
                    if (!result.Success)
                    {
                        results.Add(new FailureResultAlternative<TOutput>(result.ErrorMessage, multiResult.StartCheckpoint));
                        continue;
                    }

                    results.Add(new SuccessResultAlternative<TOutput>(result.Value, result.Consumed, state.Input.Checkpoint()));
                }

                multiResult.StartCheckpoint.Rewind();
                return new MultiResult<TOutput>(this, multiResult.Location, multiResult.StartCheckpoint, results);
            }

            public override string ToString() => DefaultStringifier.ToString(this);

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
                => Parse(state);

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

        public class MultiParser<TInput, TMiddle, TOutput> : IMultiParser<TInput, TOutput>
        {
            private readonly IMultiParser<TInput, TMiddle> _inner;
            private readonly MultiParserSelector<TInput, TMiddle, TOutput> _getParser;

            public MultiParser(IMultiParser<TInput, TMiddle> inner, MultiParserSelector<TInput, TMiddle, TOutput> getParser)
            {
                _inner = inner;
                _getParser = getParser;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                var multiResult = _inner.Parse(state);

                var results = new List<IResultAlternative<TOutput>>();

                foreach (var alt in multiResult.Results.Where(r => r.Success))
                {
                    var left = new LeftValue(alt.Value, multiResult.Location);
                    var rightParser = _getParser(left);
                    alt.Continuation.Rewind();
                    var result = rightParser.Parse(state);
                    if (!result.Success)
                    {
                        results.Add(new FailureResultAlternative<TOutput>("Right parser returned no valid results", multiResult.StartCheckpoint));
                        continue;
                    }

                    foreach (var resultAlt in result.Results)
                    {
                        results.Add(new SuccessResultAlternative<TOutput>(resultAlt.Value, resultAlt.Consumed, resultAlt.Continuation));
                    }
                }

                multiResult.StartCheckpoint.Rewind();
                return new MultiResult<TOutput>(this, multiResult.Location, multiResult.StartCheckpoint, results);
            }

            public override string ToString() => DefaultStringifier.ToString(this);

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
                => Parse(state);

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
}
