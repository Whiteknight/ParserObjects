using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Parser infrastructure to execute a sequence of parsers without constructing an object graph.
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
        private readonly IParseState<TInput> _state;

        private int _consumed;

        public State(IParseState<TInput> state)
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

        public void Fail(string error = "Fail") => throw new ParseFailedException(error);
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

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();
            try
            {
                var seqState = new State<TInput>(state);
                var result = _func(seqState);
                return state.Success(this, result, seqState.Consumed, startCheckpoint.Location);
            }
            catch (ParseFailedException spe)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                startCheckpoint.Rewind();
                if (spe.Result != null)
                {
                    var result = spe.Result;
                    state.Log(this, $"Parse failed during sequential callback: {result}\n\n{spe.StackTrace}");
                    return state.Fail(this, $"Error during parsing: {result.Parser} {result.ErrorMessage} at {result.Location}");
                }

                state.Log(this, $"Failure triggered during sequential callback: {spe.Message}");
                return state.Fail(this, $"Failure during parsing: {spe.Message}");
            }
            catch (Exception e)
            {
                startCheckpoint.Rewind();
                state.Log(this, $"Parse failed during sequential callback: {e.Message}");
                throw;
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    [Serializable]
    private class ParseFailedException : ControlFlowException
    {
        public ParseFailedException(IResult result)
        {
            Location = result.Location;
            Result = result;
        }

        public ParseFailedException(string message) : base(message)
        {
        }

        public Location? Location { get; }

        public IResult? Result { get; }
    }
}
