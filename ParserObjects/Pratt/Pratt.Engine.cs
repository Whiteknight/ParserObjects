using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public static partial class Pratt<TInput, TOutput>
    {
        /// <summary>
        /// Parse context object which allows recursing into the parser from a user-supplied
        /// callback.
        /// </summary>
        public interface IParseContext
        {
            /// <summary>
            /// Parse the next expression from the parser, using the binding power of the current
            /// parselet.
            /// </summary>
            /// <returns></returns>
            TOutput Parse();

            /// <summary>
            /// Parse the next expression from the parser using the given binding power.
            /// </summary>
            /// <param name="rbp"></param>
            /// <returns></returns>
            TOutput Parse(int rbp);

            /// <summary>
            /// Parse a value on the input, but do not return a value. If the value does not
            /// exist, the parse aborts, the input rewinds, and the parser returns to a previous
            /// level of recursion.
            /// </summary>
            /// <param name="parser"></param>
            void Expect(IParser<TInput> parser);

            TOutput Parse(IParser<TInput, TOutput> parser);

            void Fail();
        }

        // Simple contextual wrapper, so that private Engine methods can be
        // exposed to user callbacks
        private class ParseContext : IParseContext
        {
            private readonly ParseState<TInput> _state;
            private readonly Engine _engine;
            private readonly int _rbp;

            public ParseContext(ParseState<TInput> state, Engine engine, int rbp)
            {
                _state = state;
                _engine = engine;
                _rbp = rbp;
            }

            public TOutput Parse() => Parse(_rbp);

            public TOutput Parse(int rbp)
            {
                var (success, value) = _engine.Expression(_state, rbp);
                if (!success)
                    throw new ParseException();
                return value;
            }

            public void Expect(IParser<TInput> parser)
            {
                var result = parser.Parse(_state);
                if (!result.Success)
                    throw new ParseException();
            }

            public TOutput Parse(IParser<TInput, TOutput> parser)
            {
                var result = parser.Parse(_state);
                if (!result.Success)
                    throw new ParseException();
                return result.Value;
            }

            public void Fail() => throw new ParseException();
        }

        // control-flow exception type, so that errors during user-callbacks can return to the
        // engine immediately and be considered a failure of the current rule.
        [Serializable]
        private class ParseException : ControlFlowException
        {
            public ParseException() { }
            public ParseException(string message) : base(message) { }
            public ParseException(string message, Exception inner) : base(message, inner) { }
            protected ParseException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        private class Engine
        {
            private readonly IEnumerable<IParselet> _parselets;

            public Engine(IEnumerable<IParselet> parselets)
            {
                _parselets = parselets;
            }

            public (bool success, TOutput value) Parse(ParseState<TInput> state) => Expression(state, 0);

            public (bool success, TOutput value) Expression(ParseState<TInput> state, int rbp)
            {
                // Get the first "left" token. This token may have any type output by the parser
                var (hasLeftToken, leftToken, leftContext) = GetNextToken(state);
                if (!hasLeftToken)
                    return (false, default);

                // Transform the IToken into IToken<TInput> using the NullDenominator rule
                var (hasLeft, left) = leftToken.NullDenominator(leftContext);
                if (!hasLeft)
                    return (false, default);

                while (true)
                {
                    var cp = state.Input.Checkpoint();

                    // Now get the next "right" token. This token may have any type output by the
                    // parser.
                    var (hasRightToken, rightToken, rightContext) = GetNextToken(state);
                    if (!hasRightToken || rbp >= rightToken.LeftBindingPower)
                    {
                        cp.Rewind();
                        break;
                    }

                    // Transform the IToken into IToken<TOutput> using the LeftDenominator rule and
                    // the current left value
                    var (hasRight, right) = rightToken.LeftDenominator(rightContext, left);
                    if (!hasRight)
                    {
                        cp.Rewind();
                        break;
                    }

                    // Set the next left value to be the current combined right value and continue
                    // the loop
                    left = right;
                }

                return (true, left.Value);
            }

            private (bool, IToken, ParseContext) GetNextToken(ParseState<TInput> state)
            {
                foreach (var parselet in _parselets)
                {
                    var (success, token) = parselet.TryGetNext(state);
                    if (success)
                        return (true, token, new ParseContext(state, this, parselet.Rbp));
                }

                // This is both the "no match" result and also the "end of input" result
                // Either situation causes the parser to bail out.
                return (false, null, null);
            }
        }
    }
}
