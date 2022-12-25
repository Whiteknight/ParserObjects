using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class TryParser<TInput>
{
    public record Parser(
        IParser<TInput> Inner,
        Action<Exception>? Examine = null,
        bool Bubble = false,
        string Name = ""
    ) : IParser<TInput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            try
            {
                return Inner.Match(state);
            }
            catch (ControlFlowException)
            {
                // These exceptions are used within the library for non-local control flow, and
                // should not be caught or modified here.
                throw;
            }
            catch (Exception ex)
            {
                cp.Rewind();
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return false;
            }
        }

        public IResult Parse(IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            try
            {
                return Inner.Parse(state);
            }
            catch (ControlFlowException)
            {
                // These exceptions are used within the library for non-local control flow, and
                // should not be caught or modified here.
                throw;
            }
            catch (Exception ex)
            {
                cp.Rewind();
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return state.Fail(this, ex.Message, new[] { ex });
            }
        }

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);
    }

    public record Parser<TOutput>(
        IParser<TInput, TOutput> Inner,
        Action<Exception>? Examine = null,
        bool Bubble = false,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            try
            {
                return Inner.Match(state);
            }
            catch (ControlFlowException)
            {
                // These exceptions are used within the library for non-local control flow, and
                // should not be caught or modified here.
                throw;
            }
            catch (Exception ex)
            {
                cp.Rewind();
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return false;
            }
        }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            try
            {
                return Inner.Parse(state);
            }
            catch (ControlFlowException)
            {
                // These exceptions are used within the library for non-local control flow, and
                // should not be caught or modified here.
                throw;
            }
            catch (Exception ex)
            {
                cp.Rewind();
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return state.Fail(this, ex.Message, new[] { ex });
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);
    }

    public record MultiParser<TOutput>(
        IMultiParser<TInput, TOutput> Inner,
        Action<Exception>? Examine = null,
        bool Bubble = false,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            try
            {
                return Inner.Parse(state);
            }
            catch (ControlFlowException)
            {
                // These exceptions are used within the library for non-local control flow, and
                // should not be caught or modified here.
                throw;
            }
            catch (Exception ex)
            {
                cp.Rewind();
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;

                return new MultiResult<TOutput>(this, cp.Location, cp, Array.Empty<IResultAlternative<TOutput>>(), data: new[] { ex });
            }
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);
    }
}
