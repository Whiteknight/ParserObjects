using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser and related machinery for handling a right-associative parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class RightApply<TInput, TMiddle, TOutput>
    {
        /// <summary>
        /// Callback delegate for producing a result from a complete set of left, middle and right
        /// values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="middle"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public delegate TOutput Produce(TOutput left, TMiddle middle, TOutput right);

        /// <summary>
        /// Callback delegate to create a missing object.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate TOutput Create(ISequence<TInput> input, IDataStore data);

        /// <summary>
        /// Attempts to parse a right-recursive or right-associative parse rule. Useful for limited
        /// situations, especially for parsing expressions.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _item;
            private readonly IParser<TInput, TMiddle> _middle;
            private readonly Produce _produce;
            private readonly Create? _getMissingRight;
            private readonly Quantifier _quantifier;

            // <item> (<middle> <item>)* with right-associativity in the production method

            public Parser(IParser<TInput, TOutput> item, IParser<TInput, TMiddle> middle, Produce produce, Quantifier quantifier, Create? getMissingRight = null)
            {
                Assert.ArgumentNotNull(item, nameof(item));
                Assert.ArgumentNotNull(middle, nameof(middle));
                Assert.ArgumentNotNull(produce, nameof(produce));

                _item = item;
                _middle = middle;
                _produce = produce;
                _getMissingRight = getMissingRight;
                _quantifier = quantifier;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));

                var startCp = state.Input.Checkpoint();

                var leftResult = _item.Parse(state);
                if (!leftResult.Success)
                    return leftResult;

                return _quantifier switch
                {
                    Quantifier.ExactlyOne => ParseExactlyOne(state, leftResult, startCp),
                    Quantifier.ZeroOrOne => ParseZeroOrOne(state, leftResult),
                    Quantifier.ZeroOrMore => ParseZeroOrMore(state, leftResult),
                    _ => throw new InvalidOperationException("Unsupported quantifier"),
                };
            }

            private IResult<TOutput> ParseZeroOrMore(IParseState<TInput> state, IResult<TOutput> leftResult)
            {
                var resultStack = new Stack<(TOutput left, TMiddle middle)>();

                IResult<TOutput> ProduceSuccess(TOutput right, int consumed)
                {
                    while (resultStack.Count > 0)
                    {
                        var (left, middle) = resultStack.Pop();
                        right = _produce(left, middle, right);
                    }

                    return state.Success(this, right, consumed, leftResult.Location);
                }

                var left = leftResult.Value;
                int consumed = leftResult.Consumed;

                while (true)
                {
                    var checkpoint = state.Input.Checkpoint();

                    // We have the left, so parse the middle. If not found, just return left
                    var middleResult = _middle.Parse(state);
                    if (!middleResult.Success)
                        return ProduceSuccess(left, consumed);

                    int rightConsumed = middleResult.Consumed;

                    // We have <left> <middle>, now we have to look for <right>.
                    // if we have it, push state, set right as the new left, and repeat loop
                    var rightResult = _item.Parse(state);
                    if (rightResult.Success)
                    {
                        // Add left and middle to the stack, and we'll loop again with the left
                        rightConsumed += rightResult.Consumed;
                        consumed += rightConsumed;
                        resultStack.Push((left, middleResult.Value));
                        left = rightResult.Value;
                        if (rightConsumed == 0)
                            return ProduceSuccess(rightResult.Value, consumed);
                        continue;
                    }

                    // We have <left> <middle> but no <right>. See if we can make a synthetic one
                    if (_getMissingRight != null)
                    {
                        // create a synthetic right item and short-circuit exit (we could go around
                        // again, fail the <middle> and exit at that point, but this is faster and
                        // doesn't require allocating a new IResult<T>
                        consumed += middleResult.Consumed;
                        var syntheticRight = _getMissingRight(state.Input, state.Data);
                        resultStack.Push((left, middleResult.Value));
                        return ProduceSuccess(syntheticRight, consumed);
                    }

                    // We can't make a synthetic right, so rewind to give back the <middle>
                    checkpoint.Rewind();
                    return ProduceSuccess(left, consumed);
                }
            }

            private IResult<TOutput> ParseZeroOrOne(IParseState<TInput> state, IResult<TOutput> leftResult)
            {
                var checkpoint = state.Input.Checkpoint();
                var middleResult = _middle.Parse(state);
                if (!middleResult.Success)
                    return leftResult;

                var rightResult = _item.Parse(state);
                if (rightResult.Success)
                {
                    var result = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
                    return state.Success(this, result, leftResult.Consumed + middleResult.Consumed + rightResult.Consumed, leftResult.Location);
                }

                checkpoint.Rewind();

                // We have <left> <middle> but no <right>. See if we can make a synthetic one
                if (_getMissingRight != null)
                {
                    // create a synthetic right item and short-circuit exit (we could go around
                    // again, fail the <middle> and exit at that point, but this is faster and
                    // doesn't require allocating a new IResult<T>
                    var syntheticRight = _getMissingRight(state.Input, state.Data);
                    var result = _produce(leftResult.Value, middleResult.Value, syntheticRight);
                    return state.Success(this, result, leftResult.Consumed + middleResult.Consumed, leftResult.Location);
                }

                return leftResult;
            }

            private IResult<TOutput> ParseExactlyOne(IParseState<TInput> state, IResult<TOutput> leftResult, ISequenceCheckpoint startCp)
            {
                var middleCp = state.Input.Checkpoint();
                var middleResult = _middle.Parse(state);
                if (!middleResult.Success)
                    return state.Fail(this, "Expected exactly one production but found zero");

                var rightResult = _item.Parse(state);
                if (rightResult.Success)
                {
                    var result = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
                    return state.Success(this, result, leftResult.Consumed + middleResult.Consumed + rightResult.Consumed, leftResult.Location);
                }

                // We have <left> <middle> but no <right>. See if we can make a synthetic one
                if (_getMissingRight != null)
                {
                    // create a synthetic right item and short-circuit exit (we could go around
                    // again, fail the <middle> and exit at that point, but this is faster and
                    // doesn't require allocating a new IResult<T>
                    middleCp.Rewind();
                    var syntheticRight = _getMissingRight(state.Input, state.Data);
                    var result = _produce(leftResult.Value, middleResult.Value, syntheticRight);
                    return state.Success(this, result, leftResult.Consumed + middleResult.Consumed, leftResult.Location);
                }

                startCp.Rewind();
                return state.Fail(this, "Expected exactly one production but found zero");
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new IParser[] { _item, _middle };

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
