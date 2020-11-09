using System;
using System.Collections.Generic;
using ParserObjects.Sequences;
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


        public IResult<TOutput> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));

            var leftResult = _item.Parse(t);
            if (!leftResult.Success)
                return Result.Fail<TOutput>(t.CurrentLocation);

            return Parse(t, leftResult);
        }

        private IResult<TOutput> Parse(ISequence<TInput> t, IResult<TOutput> leftResult)
        {
            var window = new WindowSequence<TInput>(t);

            var middleResult = _middle.Parse(window);
            if (!middleResult.Success)
                return leftResult;

            var itemResult = _item.Parse(window);
            if (itemResult.Success)
            {
                // We don't have to use the window here, we no longer need to rewind it
                var selfResult = Parse(t, itemResult);
                var rightResult = selfResult.Success ? selfResult : itemResult;

                var value = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
                return Result.Success(value, leftResult.Location);
            }

            if (_getMissingRight != null)
            {
                var syntheticRight = _getMissingRight(window);
                var value = _produce(leftResult.Value, middleResult.Value, syntheticRight);
                return Result.Success(value, window.CurrentLocation);
            }

            window.Rewind();
            return leftResult;
        }

        IResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

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
