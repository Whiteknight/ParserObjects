using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser to help with left-associative or left-recursive parse situations. Executes an
    /// initial parser, and then passes that value to the right-hand-side production. The right
    /// value is then used as the new left value and the loop repeats.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class LeftApply<TInput, TOutput>
    {
        public delegate IParser<TInput, TOutput> GetRightFunc(IParser<TInput, TOutput> left);

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _initial;
            private readonly GetRightFunc _getRight;
            private readonly Quantifier _quantifier;
            private readonly IParser<TInput, TOutput> _right;
            private readonly LeftValueParser<TInput, TOutput> _left;

            private string _name;

            public Parser(IParser<TInput, TOutput> initial, GetRightFunc getRight, Quantifier arity)
            {
                Assert.ArgumentNotNull(initial, nameof(initial));
                Assert.ArgumentNotNull(getRight, nameof(getRight));

                _initial = initial;
                _getRight = getRight;
                _quantifier = arity;
                _left = new LeftValueParser<TInput, TOutput>();
                _right = getRight(_left);
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
                switch (_quantifier)
                {
                    case Quantifier.ExactlyOne:
                        return ParseExactlyOne(state);
                    case Quantifier.ZeroOrOne:
                        return ParseZeroOrOne(state);
                    case Quantifier.ZeroOrMore:
                        return ParseZeroOrMore(state);
                }

                return state.Fail(this, $"Quantifier value {_quantifier} not supported");
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
                // Notice that replacing the initial parser causes a close of the right parser to be
                // created. This may be unexpected for the user, but it's the only way to guarantee
                // correctness and not have shared mutable state.
                if (_initial == find && replace is IParser<TInput, TOutput> initialTyped)
                    return new Parser(initialTyped, _getRight, _quantifier);

                // Replacing _right here will detach it from _left, and it will be impossible to use
                if (_right == find)
                    ThrowCannotReplaceRightParserException();

                if (_left == find)
                    ThrowCannotReplaceLeftValueParserException();

                return this;
            }

            public override string ToString()
            {
                var typeName = this.GetType().Name;
                return Name == null ? base.ToString() : $"{typeName} {Name}";
            }

            private static void ThrowCannotReplaceRightParserException()
            {
                throw new InvalidOperationException(
                    "Cannot replace the right parser. " +
                    "The right parser must have a reference to the current left parser for " +
                    "recursion to work correctly. " +
                    "Please consider replacing the entire LeftApplyParser instead of just " +
                    "the right parser.");
            }

            private static void ThrowCannotReplaceLeftValueParserException()
            {
                throw new InvalidOperationException(
                    "Cannot replace the internal left value parser. " +
                    "This parser is for holding internal state only and should not " +
                    "be modified externally. Please create a new LeftApplyParser with " +
                    "your changes instead.");
            }
        }
    }
}
