using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public static class RightApply<TInput, TMiddle, TOutput>
    {
        public delegate TOutput Produce(TOutput left, TMiddle middle, TOutput right);

        public delegate TOutput Create(ISequence<TInput> input, IDataStore data);

        /// <summary>
        /// Attempts to parse a right-recursive or right-associative parse rule. Useful for limited
        /// situations, especially for parsing expressions
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _item;
            private readonly IParser<TInput, TMiddle> _middle;
            private readonly Produce _produce;
            private readonly Create _getMissingRight;
            private readonly Quantifier _quantifier;

            // <item> (<middle> <item>)* with right-associativity in the production method

            public Parser(IParser<TInput, TOutput> item, IParser<TInput, TMiddle> middle, Produce produce, Quantifier quantifier, Create getMissingRight = null)
            {
                Assert.ArgumentNotNull(item, nameof(item));
                Assert.ArgumentNotNull(middle, nameof(middle));
                Assert.ArgumentNotNull(produce, nameof(produce));

                _item = item;
                _middle = middle;
                _produce = produce;
                _getMissingRight = getMissingRight;
                _quantifier = quantifier;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));

                var leftResult = _item.Parse(state);
                if (!leftResult.Success)
                    return leftResult;

                return _quantifier switch
                {
                    Quantifier.ExactlyOne => ParseExactlyOne(state, leftResult),
                    Quantifier.ZeroOrOne => ParseZeroOrOne(state, leftResult),
                    Quantifier.ZeroOrMore => ParseZeroOrMore(state, leftResult),
                    _ => throw new InvalidOperationException("Unsupported quantifier"),
                };
            }

            private IResult<TOutput> ParseZeroOrMore(ParseState<TInput> state, IResult<TOutput> leftResult)
            {
                var resultStack = new Stack<(TOutput left, TMiddle middle)>();

                IResult<TOutput> produceSuccess(TOutput right)
                {
                    while (resultStack.Count > 0)
                    {
                        var (left, middle) = resultStack.Pop();
                        right = _produce(left, middle, right);
                    }
                    return state.Success(this, right, state.Input.CurrentLocation);
                }

                var left = leftResult.Value;

                while (true)
                {
                    var checkpoint = state.Input.Checkpoint();

                    // We have the left, so parse the middle. If not found, just return left
                    var middleResult = _middle.Parse(state);
                    if (!middleResult.Success)
                        return produceSuccess(left);

                    // We have <left> <middle>, now we have to look for <right>.
                    // if we have it, push state, set right as the new left, and repeat loop
                    var rightResult = _item.Parse(state);
                    if (rightResult.Success)
                    {
                        // Add left and middle to the stack, and we'll loop again with the left
                        resultStack.Push((left, middleResult.Value));
                        left = rightResult.Value;
                        continue;
                    }

                    // We have <left> <middle> but no <right>. See if we can make a synthetic one
                    if (_getMissingRight != null)
                    {
                        // create a synthetic right item and short-circuit exit (we could go around
                        // again, fail the <middle> and exit at that point, but this is faster and
                        // doesn't require allocating a new IResult<T>
                        var syntheticRight = _getMissingRight(state.Input, state.Data);
                        resultStack.Push((left, middleResult.Value));
                        return produceSuccess(syntheticRight);
                    }

                    // We can't make a synthetic right, so rewind to give back the <middle>
                    checkpoint.Rewind();
                    return produceSuccess(left);
                }
            }

            private IResult<TOutput> ParseZeroOrOne(ParseState<TInput> state, IResult<TOutput> leftResult)
            {
                var checkpoint = state.Input.Checkpoint();
                var middleResult = _middle.Parse(state);
                if (!middleResult.Success)
                    return leftResult;

                var rightResult = _item.Parse(state);
                if (rightResult.Success)
                {
                    var result = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
                    return state.Success(this, result);
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
                    return state.Success(this, result);
                }

                return leftResult;
            }

            private IResult<TOutput> ParseExactlyOne(ParseState<TInput> state, IResult<TOutput> leftResult)
            {
                var checkpoint = state.Input.Checkpoint();
                var middleResult = _middle.Parse(state);
                if (!middleResult.Success)
                    return state.Fail(this, "Expected exactly one production but found zero");

                var rightResult = _item.Parse(state);
                if (rightResult.Success)
                {
                    var result = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
                    return state.Success(this, result);
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
                    return state.Success(this, result);
                }

                return state.Fail(this, "Expected exactly one production but found zero");
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new IParser[] { _item, _middle };

            public override string ToString() => ParserDefaultStringifier.ToString(this);
        }
    }
}
