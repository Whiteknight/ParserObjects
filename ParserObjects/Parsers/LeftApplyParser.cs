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

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _left.Name = string.IsNullOrEmpty(_name) ? null : _name;
            }
        }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            switch (_arity)
            {
                case ApplyArity.ExactlyOne:
                    return ParseExactlyOne(state);
                case ApplyArity.ZeroOrOne:
                    return ParseZeroOrOne(state);
                case ApplyArity.ZeroOrMore:
                    return ParseZeroOrMore(state);
            }

            return state.Fail(this, "Unknown arity value");
        }

        private IResult<TOutput> ParseExactlyOne(ParseState<TInput> state)
        {
            // Parse the left. Parse the right exactly once. Return the result
            var checkpoint = state.Input.Checkpoint();

            var leftResult = _initial.Parse(state);
            if (!leftResult.Success)
                return leftResult;

            _left.Value = leftResult.Value;
            _left.Location = leftResult.Location;

            var rightResult = _right.Parse(state);
            if (!rightResult.Success)
            {
                checkpoint.Rewind();
                return state.Fail(this, "Expected exactly one right-hand side, but right parser failed: " + rightResult.Message, rightResult.Location);
            }

            return rightResult;
        }

        private IResult<TOutput> ParseZeroOrMore(ParseState<TInput> state)
        {
            // Parse <left> then attempt to parse <right> in a loop. If <right> fails at any
            // point, return whatever is the last value we had
            var result = _initial.Parse(state);
            if (!result.Success)
                return result;

            var current = result.Value;
            _left.Value = result.Value;
            _left.Location = result.Location;
            while (true)
            {
                var rhsResult = _right.Parse(state);
                if (!rhsResult.Success)
                    return state.Success(this, current, result.Location);

                current = rhsResult.Value;
                _left.Value = current;
            }
        }

        private IResult<TOutput> ParseZeroOrOne(ParseState<TInput> state)
        {
            // Parse the left. Maybe parse the right. If <right>, return it. Otherwise <left>
            var leftResult = _initial.Parse(state);
            if (!leftResult.Success)
                return leftResult;

            _left.Value = leftResult.Value;
            _left.Location = leftResult.Location;

            var rightResult = _right.Parse(state);
            return rightResult.Success ? rightResult : leftResult;
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

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
