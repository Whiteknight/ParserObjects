﻿using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _item;
        private readonly IParser<TInput, TMiddle> _middle;
        private readonly Func<TOutput, TMiddle, TOutput, TOutput> _produce;
        private readonly Func<ISequence<TInput>, TOutput> _getMissingRight;

        // <item> (<middle> <item>)* with right-associativity in the production method

        public RightApplyZeroOrMoreParser(IParser<TInput, TOutput> item, IParser<TInput, TMiddle> middle, Func<TOutput, TMiddle, TOutput, TOutput> produce, Func<ISequence<TInput>, TOutput> getMissingRight = null)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(middle, nameof(middle));
            Assert.ArgumentNotNull(produce, nameof(produce));

            _item = item;
            _middle = middle;
            _produce = produce;
            _getMissingRight = getMissingRight;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var leftResult = _item.Parse(state);
            if (!leftResult.Success)
                return leftResult;
            var left = leftResult.Value;

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
                    var syntheticRight = _getMissingRight(state.Input);
                    resultStack.Push((left, middleResult.Value));
                    return produceSuccess(syntheticRight);
                }

                // We can't make a synthetic right, so rewind to give back the <middle>
                checkpoint.Rewind();
                return produceSuccess(left);
            }
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _item, _middle };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_item == find && replace is IParser<TInput, TOutput> itemTyped)
                return new RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput>(itemTyped, _middle, _produce);
            if (_middle == find && replace is IParser<TInput, TMiddle> middleTyped)
                return new RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput>(_item, middleTyped, _produce);

            return this;
        }
    }
}
