using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser type that allows executing arbitrary code before and after the parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Context<TInput>
{
    public sealed record Parser<TOutput>(
        IParser<TInput, TOutput> Inner,
        Action<IParseState<TInput>>? Setup,
        Action<IParseState<TInput>>? Cleanup,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            try
            {
                Setup?.Invoke(state);
            }
            catch
            {
                return false;
            }

            bool result = false;
            try
            {
                result = Inner.Match(state);
            }
            finally
            {
                Cleanup?.Invoke(state);
            }

            if (!result)
                startCp.Rewind();
            return result;
        }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            // If setup fails, we return failure
            // If the Inner parser throws an exception we need to attempt to invoke Cleanup, then bubble
            // If the cleanup throws an exception, we allow it to bubble up (without cleanup, the parse
            // may be in an invalid state).

            var startCp = state.Input.Checkpoint();

            // If Setup() throws, return a failure
            try
            {
                Setup?.Invoke(state);
            }
            catch (Exception setupException)
            {
                return state.Fail(this, "Setup code threw an exception", new[] { setupException });
            }

            // Invoke inner parser and then Cleanup().
            // If either of these throw, we need to bubble the exception upwards so the user can
            // see it (or deal with it in a Try())
            IResult<TOutput>? result = null;
            try
            {
                result = Inner.Parse(state);
            }
            finally
            {
                Cleanup?.Invoke(state);
            }

            // At this point we haven't had an exception, so we can deal with normal success/failure
            // outcomes.
            if (result?.Success != true)
                startCp.Rewind();
            return result!;
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public sealed record MultiParser<TOutput>(
        IMultiParser<TInput, TOutput> Inner,
        Action<IParseState<TInput>>? Setup,
        Action<IParseState<TInput>>? Cleanup,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            // If Setup() throws, return a failure
            try
            {
                Setup?.Invoke(state);
            }
            catch (Exception setupException)
            {
                return new MultiResult<TOutput>(this, startCp, Array.Empty<IResultAlternative<TOutput>>(), new[] { setupException });
            }

            // Invoke inner parser and then Cleanup().
            // If either of these throw, we need to bubble the exception upwards so the user can
            // see it (or deal with it in a Try())
            IMultiResult<TOutput>? result = null;
            try
            {
                result = Inner.Parse(state);
            }
            finally
            {
                Cleanup?.Invoke(state);
            }

            // At this point we haven't had an exception, so we can deal with normal success/failure
            // outcomes.
            if (result?.Success != true)
                startCp.Rewind();
            return result!;
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
