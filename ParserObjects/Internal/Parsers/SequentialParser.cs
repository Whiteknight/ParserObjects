using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

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
    public readonly struct State<TInput>
    {
        private readonly IParseState<TInput> _state;
        private readonly SequenceCheckpoint _startCheckpoint;

        public State(IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
        {
            _state = state;
            _startCheckpoint = startCheckpoint;
        }

        /// <summary>
        /// Gets the contextual state data.
        /// </summary>
        public IDataStore Data => _state.Data;

        /// <summary>
        /// Gets the input sequence.
        /// </summary>
        public ISequence<TInput> Input => _state.Input;

        /// <summary>
        /// Invoke the parser. Exit the Sequential if the parse fails.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        /// <exception cref="ParseFailedException">Exits the Sequential if the parse fails.</exception>
        public TOutput Parse<TOutput>(IParser<TInput, TOutput> p, string errorMessage = "")
        {
            var result = p.Parse(_state);
            if (!result.Success)
                throw new ParseFailedException(result, errorMessage);
            return result.Value;
        }

        /// <summary>
        /// Attempt to invoke the parser. Return a result indicating success or failure.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public IResult<TOutput> TryParse<TOutput>(IParser<TInput, TOutput> p)
            => p.Parse(_state);

        /// <summary>
        /// Invoke the parser to match. Returns true, and consumes input, if the match succeeds.
        /// Returns false and consumes no input otherwise.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Match(IParser<TInput> p) => p.Match(_state);

        /// <summary>
        /// Invoke the parser to match. Fails the entire Sequential if the match fails.
        /// </summary>
        /// <param name="p"></param>
        /// <exception cref="ParseFailedException">Immediately exits the Sequential.</exception>
        public void Expect(IParser<TInput> p)
        {
            var ok = p.Match(_state);
            if (!ok)
                throw new ParseFailedException("Expect failed");
        }

        /// <summary>
        /// Attempt to invoke the parser but consume no input. Returns the result of the parser.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public IResult<TOutput> TestParse<TOutput>(IParser<TInput, TOutput> p)
        {
            var checkpoint = _state.Input.Checkpoint();
            var result = p.Parse(_state);
            checkpoint.Rewind();
            return result;
        }

        /// <summary>
        /// Unconditional failure. Exit the Sequential.
        /// </summary>
        /// <param name="error"></param>
        /// <exception cref="ParseFailedException">Immediately exits the Sequential.</exception>
        public void Fail(string error = "Fail") => throw new ParseFailedException(error);

        /// <summary>
        /// Get an array of all inputs consumed by the Sequential so far.
        /// </summary>
        /// <returns></returns>
        public TInput[] GetCapturedInputs()
        {
            var currentCp = _state.Input.Checkpoint();
            return _state.Input.GetBetween(_startCheckpoint, currentCp);
        }
    }

    /// <summary>
    /// Parser for sequential callbacks. Executes a specially-structured callback with a state
    /// object so the user can control the flow of data between parsers and set breakpoints
    /// during debugging.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public sealed record Parser<TInput, TOutput>(
        Func<State<TInput>, TOutput> Function,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();
            try
            {
                var seqState = new State<TInput>(state, startCheckpoint);
                var result = Function(seqState);
                var endConsumed = state.Input.Consumed;
                return state.Success(this, result, endConsumed - startCheckpoint.Consumed, startCheckpoint.Location);
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

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCheckpoint = state.Input.Checkpoint();
            try
            {
                var seqState = new State<TInput>(state, startCheckpoint);
                Function(seqState);
                return true;
            }
            catch (ParseFailedException)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                return false;
            }
            catch
            {
                startCheckpoint.Rewind();
                throw;
            }
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString("Sequential", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }

    [Serializable]
    private class ParseFailedException : ControlFlowException
    {
        public ParseFailedException(string message)
            : base(message)
        {
        }

        public ParseFailedException(IResult result, string message)
            : base(message)
        {
            Result = result;
        }

        public IResult? Result { get; }
    }
}
