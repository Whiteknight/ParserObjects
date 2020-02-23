using System;
using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers
{
    public class RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _item;
        private readonly IParser<TInput, TMiddle> _middle;
        private readonly Func<TOutput, TMiddle, TOutput, TOutput> _produce;

        // <item> (<middle> <item>)* with right-associativity in the production method

        public RightApplyZeroOrMoreParser(IParser<TInput, TOutput> item, IParser<TInput, TMiddle> middle, Func<TOutput, TMiddle, TOutput, TOutput> produce)
        {
            _item = item;
            _middle = middle;
            _produce = produce;
        }


        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var leftResult = _item.Parse(t);
            if (!leftResult.Success)
                return new FailResult<TOutput>(t.CurrentLocation);

            return Parse(t, leftResult);
        }

        private IParseResult<TOutput> Parse(ISequence<TInput> t, IParseResult<TOutput> leftResult)
        {
            var window = new WindowSequence<TInput>(t);
            var middleResult = _middle.Parse(window);
            if (!middleResult.Success)
                return leftResult;

            var itemResult = _item.Parse(window);
            if (!itemResult.Success)
            {
                window.Rewind();
                return leftResult;
            }

            // We don't have to use the window here, we no longer need to rewind it
            var selfResult = Parse(t, itemResult);
            var rightResult = selfResult.Success ? selfResult : itemResult;
            
            var value = _produce(leftResult.Value, middleResult.Value, rightResult.Value);
            return new SuccessResult<TOutput>(value, leftResult.Location);
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

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

        public IParser Accept(IParserVisitor visitor) => (visitor as ICoreVisitorDispatcher)?.VisitRightApplyZeroOrMore(this) ?? this;
    }
}
