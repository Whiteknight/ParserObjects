using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Internal.Assert;

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
    /* We have two implementations the first is ContinueWith multi->single and the other is
     * ContinueWith multi->multi. The former acts like a LINQ.Select() and the later acts like a
     * LINQ.SelectMany(). They are conceptually similar but very different in implementation.
     */

    public sealed class SingleParser : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly LeftValue<TInput, TMiddle> _left;
        private readonly IParser<TInput, TOutput> _right;
        private readonly GetParserFromParser<TInput, TMiddle, TOutput> _getParser;

        public SingleParser(IMultiParser<TInput, TMiddle> inner, GetParserFromParser<TInput, TMiddle, TOutput> getParser, string name = "")
        {
            _inner = NotNull(inner);
            _left = new LeftValue<TInput, TMiddle>(name);
            _right = getParser(_left);
            _getParser = NotNull(getParser);
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => [_inner, _right];

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            var innerResult = _inner.Parse(state);

            var results = new List<Alternative<TOutput>>();

            foreach (var alt in innerResult.Results)
            {
                if (!alt.Success)
                    continue;

                alt.Continuation.Rewind();
                _left.Value = alt.Value;
                var result = _right.Parse(state);
                results.Add(result.Success
                    ? new Alternative<TOutput>(true, null, result.Value, result.Consumed, state.Input.Checkpoint())
                    : new Alternative<TOutput>(false, result.ErrorMessage, default, 0, startCp));
            }

            startCp.Rewind();
            return new MultiResult<TOutput>(this, results);
        }

        public override string ToString() => DefaultStringifier.ToString("ContinueWith", Name, Id);

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state).AsObject();

        public INamed SetName(string name) => new SingleParser(_inner, _getParser, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IMultiPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TMiddle> _inner;
        private readonly LeftValue<TInput, TMiddle> _left;
        private readonly IMultiParser<TInput, TOutput> _right;
        private readonly GetMultiParserFromParser<TInput, TMiddle, TOutput> _getParser;

        public MultiParser(IMultiParser<TInput, TMiddle> inner, GetMultiParserFromParser<TInput, TMiddle, TOutput> getParser, string name = "")
        {
            _inner = NotNull(inner);
            _left = new LeftValue<TInput, TMiddle>(name);
            _right = getParser(_left);
            _getParser = NotNull(getParser);
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => [_inner, _right];

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            var innerResult = _inner.Parse(state);

            var results = new List<Alternative<TOutput>>();

            foreach (var alt in innerResult.Results)
            {
                if (!alt.Success)
                    continue;

                alt.Continuation.Rewind();
                _left.Value = alt.Value;
                var result = _right.Parse(state);
                if (!result.Success)
                {
                    results.Add(Alternative<TOutput>.Failure("Right parser returned no valid results", startCp));
                    continue;
                }

                results.AddRange(result.Results.Where(r => r.Success));
            }

            startCp.Rewind();
            return new MultiResult<TOutput>(this, results);
        }

        public override string ToString() => DefaultStringifier.ToString("ContinueWith", Name, Id);

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state)
            => Parse(state).AsObject();

        public INamed SetName(string name) => new MultiParser(_inner, _getParser, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IMultiPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
