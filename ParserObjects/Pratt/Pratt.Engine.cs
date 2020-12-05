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

            TValue Parse<TValue>(IParser<TInput, TValue> parser);

            /// <summary>
            /// Parse a value on the input, but do not return a value. If the value does not
            /// exist, the parse aborts, the input rewinds, and the parser returns to a previous
            /// level of recursion.
            /// </summary>
            /// <param name="parser"></param>
            void Expect(IParser<TInput> parser);

            (bool success, TOutput value) TryParse();
            (bool success, TOutput value) TryParse(int rbp);
            (bool success, TValue value) TryParse<TValue>(IParser<TInput, TValue> parser);

            void FailRule(string message = null);
            void FailLevel(string message = null);
            void FailAll(string message = null);
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

            public IDataStore Data => _state.Data;

            public TOutput Parse() => Parse(_rbp);

            public TOutput Parse(int rbp)
            {
                var (success, value, error) = _engine.Expression(_state, rbp);
                if (!success)
                    throw new ParseException(ParseExceptionSeverity.Rule, error, null, null);
                return value;
            }

            public void Expect(IParser<TInput> parser)
            {
                var result = parser.Parse(_state);
                if (!result.Success)
                    throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
            }

            public TValue Parse<TValue>(IParser<TInput, TValue> parser)
            {
                var result = parser.Parse(_state);
                if (!result.Success)
                    throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
                return result.Value;
            }

            public void FailRule(string message = null) => throw new ParseException(ParseExceptionSeverity.Rule, message ?? "Fail", null, _state.Input.CurrentLocation);
            public void FailLevel(string message = null) => throw new ParseException(ParseExceptionSeverity.Level, message ?? "", null, _state.Input.CurrentLocation);
            public void FailAll(string message = null) => throw new ParseException(ParseExceptionSeverity.Parser, message ?? "", null, _state.Input.CurrentLocation);

            public (bool success, TOutput value) TryParse()
            {
                var (success, value, _) = _engine.Expression(_state, _rbp);
                return (success, value);
            }

            public (bool success, TOutput value) TryParse(int rbp)
            {
                var (success, value, _) = _engine.Expression(_state, rbp);
                return (success, value);
            }

            public (bool success, TValue value) TryParse<TValue>(IParser<TInput, TValue> parser)
            {
                var result = parser.Parse(_state);
                return (_, _) = result;
            }
        }

        // control-flow exception type, so that errors during user-callbacks can return to the
        // engine immediately and be considered a failure of the current rule.

        private enum ParseExceptionSeverity
        {
            // Fail the current rule, but allow the engine to continue attempting to fill in the
            // current level of precidence
            Rule,

            // Fail the current precidence level
            Level,

            // Fail the entire Pratt.Parse() attempt
            Parser
        }

        [Serializable]
        private class ParseException : ControlFlowException
        {
            public ParseException()
            {
            }

            public ParseException(string message) : base(message)
            {
            }

            public ParseException(string message, Exception inner)
                : base(message, inner)
            {
            }

            public ParseException(ParseExceptionSeverity severity, string message, IParser parser, Location location)
                : this(message)
            {
                Severity = severity;
                Parser = parser;
                Location = location;
            }

            protected ParseException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            {
            }

            public ParseExceptionSeverity Severity { get; }
            public IParser Parser { get; }
            public Location Location { get; }
        }

        private class Engine
        {
            private readonly IEnumerable<IParselet> _parselets;

            public Engine(IEnumerable<IParselet> parselets)
            {
                _parselets = parselets;
            }

            public (bool success, TOutput value, string error) Parse(ParseState<TInput> state) => Expression(state, 0);

            public (bool success, TOutput value, string error) Expression(ParseState<TInput> state, int rbp)
            {
                var levelCp = state.Input.Checkpoint();
                try
                {
                    // Get the first "left" token. This token may have any type output by the parser
                    var (hasLeftToken, leftToken, leftContext) = GetNextToken(state);
                    if (!hasLeftToken)
                        return (false, default, "Could not match any tokens");

                    // Transform the IToken into IToken<TInput> using the NullDenominator rule
                    var (hasLeft, left) = leftToken.NullDenominator(leftContext);
                    if (!hasLeft)
                        return (false, default, "Left Denominator failed");

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

                    return (true, left.Value, null);
                }
                catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
                {
                    levelCp.Rewind();
                    return (false, default, pe.Message ?? "Fail");
                }
            }

            private (bool, IToken, ParseContext) GetNextToken(ParseState<TInput> state)
            {
                foreach (var parselet in _parselets)
                {
                    var (success, token) = parselet.TryGetNext(state);
                    if (success)
                        return (true, token, new ParseContext(state, this, parselet.Rbp));
                }

                // This is both the "no match" result. It will also be the "end of input" result
                // unless an explicit "end of input" parselet has been added.
                // TODO: Need to test what happens if we have an end-of-input parselet added,
                // which will continue to succeed and consume zero input. Will probably break the
                // loop because it has the same binding power.
                return (false, null, null);
            }
        }
    }
}
