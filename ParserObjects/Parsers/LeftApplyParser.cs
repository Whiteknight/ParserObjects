using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public enum ApplyArity
    {
        ZeroOrOne,
        ExactlyOne,
        ZeroOrMore
    }

    public class LeftApplyParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _initial;
        private readonly ApplyArity _arity;
        private readonly IParser<TInput, TOutput> _right;
        private readonly LeftValueParser<TInput, TOutput> _left;

        private string _name;

        public LeftApplyParser(IParser<TInput, TOutput> initial, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> getRight, ApplyArity arity)
        {
            Assert.ArgumentNotNull(initial, nameof(initial));
            Assert.ArgumentNotNull(getRight, nameof(getRight));

            _initial = initial;
            _arity = arity;
            _left = new LeftValueParser<TInput, TOutput>();
            _right = getRight(_left);
        }

        private LeftApplyParser(IParser<TInput, TOutput> initial, LeftValueParser<TInput, TOutput> left, IParser<TInput, TOutput> right, ApplyArity arity)
        {
            _initial = initial;
            _left = left;
            _right = right;
            _arity = arity;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            switch (_arity)
            {
                case ApplyArity.ExactlyOne:
                    return ParseExactlyOne(t);
                case ApplyArity.ZeroOrOne:
                    return ParseZeroOrOne(t);
                case ApplyArity.ZeroOrMore:
                    return ParseZeroOrMore(t);
            }

            return new FailResult<TOutput>(t.CurrentLocation);
        }

        private IParseResult<TOutput> ParseExactlyOne(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var window = t.Window();
            var leftResult = _initial.Parse(window);
            if (!leftResult.Success)
                return new FailResult<TOutput>(window.CurrentLocation);

            _left.Value = leftResult.Value;
            _left.Location = leftResult.Location;

            var rightResult = _right.Parse(window);
            if (rightResult.Success)
                return rightResult;

            window.Rewind();
            return new FailResult<TOutput>(t.CurrentLocation);
        }

        private IParseResult<TOutput> ParseZeroOrMore(ISequence<TInput> t)
        {
            var result = _initial.Parse(t);
            if (!result.Success)
                return new FailResult<TOutput>(t.CurrentLocation);

            var current = result.Value;
            _left.Value = result.Value;
            _left.Location = result.Location;
            while (true)
            {
                var rhsResult = _right.Parse(t);
                if (!rhsResult.Success)
                    return new SuccessResult<TOutput>(current, result.Location);

                current = rhsResult.Value;
                _left.Value = current;
            }
        }

        private IParseResult<TOutput> ParseZeroOrOne(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var leftResult = _initial.Parse(t);
            if (!leftResult.Success)
                return new FailResult<TOutput>(t.CurrentLocation);

            _left.Value = leftResult.Value;
            _left.Location = leftResult.Location;

            var rightResult = _right.Parse(t);
            if (rightResult.Success)
                return rightResult;
            return leftResult;
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _left.Name = string.IsNullOrEmpty(_name) ? null : _name;
            }
        }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find && replace is IParser<TInput, TOutput> initialTyped)
                return new LeftApplyParser<TInput, TOutput>(initialTyped, _left, _right, _arity);

            if (_right == find && replace is IParser<TInput, TOutput> rightTyped)
                return new LeftApplyParser<TInput, TOutput>(_initial, _left, rightTyped, _arity);

            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
