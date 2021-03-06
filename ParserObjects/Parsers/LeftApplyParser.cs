﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser to help with left-associative or left-recursive parse situations. Executes an
    /// initial parser, and then passes that value to the right-hand-side production. The right
    /// value is then used as the new left value and the loop repeats. Contains the parser and
    /// related machinery.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class LeftApply<TInput, TOutput>
    {
        public delegate IParser<TInput, TOutput> GetRightFunc(IParser<TInput, TOutput> left);

        /// <summary>
        /// The left-apply parser, which handles left-associative parses without recursion.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _initial;
            private readonly Quantifier _quantifier;
            private readonly IParser<TInput, TOutput> _right;
            private readonly LeftValue _left;

            private string _name;

            public Parser(IParser<TInput, TOutput> initial, GetRightFunc getRight, Quantifier arity)
            {
                Assert.ArgumentNotNull(initial, nameof(initial));
                Assert.ArgumentNotNull(getRight, nameof(getRight));

                _initial = initial;
                _quantifier = arity;
                _left = new LeftValue();
                _right = getRight(_left);
                _name = string.Empty;
            }

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    _left.Name = string.IsNullOrEmpty(_name) ? "LEFT" : _name;
                }
            }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                return _quantifier switch
                {
                    Quantifier.ExactlyOne => ParseExactlyOne(state),
                    Quantifier.ZeroOrOne => ParseZeroOrOne(state),
                    Quantifier.ZeroOrMore => ParseZeroOrMore(state),
                    _ => state.Fail(this, $"Quantifier value {_quantifier} not supported"),
                };
            }

            private IResult<TOutput> ParseExactlyOne(IParseState<TInput> state)
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
                    return state.Fail(this, "Expected exactly one right-hand side, but right parser failed: " + rightResult.ErrorMessage, rightResult.Location);
                }

                return state.Success(this, rightResult.Value, leftResult.Consumed + rightResult.Consumed, leftResult.Location);
            }

            private IResult<TOutput> ParseZeroOrMore(IParseState<TInput> state)
            {
                // Parse <left> then attempt to parse <right> in a loop. If <right> fails at any
                // point, return whatever is the last value we had
                var result = _initial.Parse(state);
                if (!result.Success)
                    return result;

                var current = result.Value;
                _left.Value = result.Value;
                _left.Location = result.Location;
                int consumed = result.Consumed;
                while (true)
                {
                    var rhsResult = _right.Parse(state);
                    if (!rhsResult.Success)
                        return state.Success(this, current, consumed, result.Location);

                    consumed += rhsResult.Consumed;
                    current = rhsResult.Value;
                    _left.Value = current;
                }
            }

            private IResult<TOutput> ParseZeroOrOne(IParseState<TInput> state)
            {
                // Parse the left. Maybe parse the right. If <right>, return it. Otherwise <left>
                var leftResult = _initial.Parse(state);
                if (!leftResult.Success)
                    return leftResult;

                _left.Value = leftResult.Value;
                _left.Location = leftResult.Location;

                var rightResult = _right.Parse(state);
                if (!rightResult.Success)
                    return leftResult;
                return state.Success(this, rightResult.Value, leftResult.Consumed + rightResult.Consumed, leftResult.Location);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        private class LeftValue : IParser<TInput, TOutput>
        {
            public LeftValue()
            {
                Name = "LEFT";
            }

            public TOutput? Value { get; set; }

            public Location? Location { get; set; }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location!);

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location!);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
