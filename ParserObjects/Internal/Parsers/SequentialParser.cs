using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser infrastructure to execute a sequence of parsers without constructing an object graph.
/// </summary>
public static class Sequential
{
    public class ParseFailedException : ControlFlowException
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

    /// <summary>
    /// Parser for sequential callbacks. Executes a specially-structured callback with a state
    /// object so the user can control the flow of data between parsers and set breakpoints
    /// during debugging.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public sealed record Parser<TInput, TOutput>(
        Func<SequentialState<TInput>, TOutput> Function,
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
                var seqState = new SequentialState<TInput>(state, startCheckpoint);
                var result = Function(seqState);
                var endConsumed = state.Input.Consumed;
                return state.Success(this, result, endConsumed - startCheckpoint.Consumed);
            }
            catch (ParseFailedException spe)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                var location = state.Input.CurrentLocation;
                startCheckpoint.Rewind();
                if (spe.Result != null)
                {
                    var result = spe.Result;
                    state.Log(this, $"Parse failed during sequential callback: {result}\n\n{spe.StackTrace}");
                    return state.Fail(this, $"Error during parsing: {result.Parser} {result.ErrorMessage} at {location}");
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
                var seqState = new SequentialState<TInput>(state, startCheckpoint);
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

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IFunctionPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
