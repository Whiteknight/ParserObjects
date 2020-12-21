using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Sequential parser and related classes.
    /// </summary>
    public static class Sequential
    {
        /// <summary>
        /// State object for a sequential parse. Handles control flow and input sequence
        /// management.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        public class State<TInput>
        {
            private readonly ParseState<TInput> _state;

            private int _consumed;

            public State(ParseState<TInput> state)
            {
                _state = state;
                _consumed = 0;
            }

            public IDataStore Data => _state.Data;

            public ISequence<TInput> Input => _state.Input;

            public int Consumed => _consumed;

            public TOutput Parse<TOutput>(IParser<TInput, TOutput> p)
            {
                var result = p.Parse(_state);
                if (!result.Success)
                    throw new ParseFailedException(result);
                _consumed += result.Consumed;
                return result.Value;
            }

            public IResult<TOutput> TryParse<TOutput>(IParser<TInput, TOutput> p)
            {
                var result = p.Parse(_state);
                if (result.Success)
                    _consumed += result.Consumed;
                return result;
            }

            public IResult<TOutput> TryMatch<TOutput>(IParser<TInput, TOutput> p)
            {
                var checkpoint = _state.Input.Checkpoint();
                var result = p.Parse(_state);
                checkpoint.Rewind();
                return result;
            }
        }

        [Serializable]
        private class ParseFailedException : ControlFlowException
        {
            public ParseFailedException()
            {
            }

            public ParseFailedException(IResult result)
            {
                Location = result.Location;
                Result = result;
            }

            protected ParseFailedException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            public ParseFailedException(string message) : base(message)
            {
            }

            public ParseFailedException(string message, Exception inner) : base(message, inner)
            {
            }

            public Location? Location { get; }

            public IResult? Result { get; }
        }

        /// <summary>
        /// Parser for sequential callbacks. Executes a specially-structured callback with a state
        /// object so the user can control the flow of data between parsers and set breakpoints
        /// during debugging.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        public class Parser<TInput, TOutput> : IParser<TInput, TOutput>
        {
            private readonly Func<State<TInput>, TOutput> _func;

            public Parser(Func<State<TInput>, TOutput> func)
            {
                _func = func;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startLocation = state.Input.CurrentLocation;
                var checkpoint = state.Input.Checkpoint();
                try
                {
                    var seqState = new State<TInput>(state);
                    var result = _func(seqState);
                    return state.Success(this, result, seqState.Consumed, startLocation);
                }
                catch (ParseFailedException spe)
                {
                    // This exception is part of normal flow-control for this parser
                    // Other exceptions bubble up like normal.
                    checkpoint.Rewind();
                    var result = spe.Result!;
                    state.Log(this, $"Parse failed during sequential callback: {result}\n\n{spe.StackTrace}");
                    return state.Fail(this, $"Error during parsing: {result.Parser} {result.ErrorMessage} at {result.Location}");
                }
                catch (Exception e)
                {
                    checkpoint.Rewind();
                    state.Log(this, $"Parse failed during sequential callback: {e.Message}");
                    throw;
                }
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
