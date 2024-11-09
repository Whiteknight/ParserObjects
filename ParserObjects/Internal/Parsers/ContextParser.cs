using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser type that allows executing arbitrary code before and after the parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Context<TInput>
{
    /* Basic flow goes like this:
     * 1. Run setup callback.
     *      a. If setup fails, we catch it, rewind any consumed input, and return a nice failure message.
     * 2. Execute the inner parser in a try{}
     *      a. If the Inner parser throws we attempt to invoke Cleanup, then allow the exception to bubble up.
     *      b. We do not explicitly rewind here, we want to leave the input in the state it was
     *         when the exception was thrown. Try() parser may rewind it.
     * 3. If the cleanup throws an exception, we allow it to bubble up (without cleanup, the parse
     *    may be in an invalid state).
     */

    public sealed class Parser : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;
        private readonly Action<ParseContext<TInput, object>>? _setup;
        private readonly Action<ParseContext<TInput, object>>? _cleanup;

        public Parser(
            IParser<TInput> inner,
            Action<ParseContext<TInput, object>>? setup,
            Action<ParseContext<TInput, object>>? cleanup,
            string name = ""
        )
        {
            _inner = inner;
            _setup = setup;
            _cleanup = cleanup;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public bool Match(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            try
            {
                _setup?.Invoke(new ParseContext<TInput, object>(_inner, state, default));
            }
            catch
            {
                startCp.Rewind();
                return false;
            }

            bool result = false;
            try
            {
                result = _inner.Match(state);
            }
            finally
            {
                _cleanup?.Invoke(new ParseContext<TInput, object>(_inner, state, default));
            }

            return result;
        }

        public Result<object> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            try
            {
                _setup?.Invoke(new ParseContext<TInput, object>(_inner, state, default));
            }
            catch (Exception setupException)
            {
                return state.Fail(this, "Setup code threw an exception", new ResultData(setupException));
            }

            Result<object> result = default;
            try
            {
                result = _inner.Parse(state);
            }
            finally
            {
                _cleanup?.Invoke(new ParseContext<TInput, object>(_inner, state, result));
            }

            // If the callbacks consumed any input, be clear about how much we've consumed and who
            // consumed it.
            if (!result.Success)
                startCp.Rewind();
            var totalConsumed = state.Input.Consumed - startCp.Consumed;
            return totalConsumed == result.Consumed
                ? result
                : result with { Consumed = totalConsumed, Parser = this };
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public INamed SetName(string name) => new Parser(_inner, _setup, _cleanup, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;
        private readonly Action<ParseContext<TInput, TOutput>>? _setup;
        private readonly Action<ParseContext<TInput, TOutput>>? _cleanup;

        public Parser(
            IParser<TInput, TOutput> inner,
            Action<ParseContext<TInput, TOutput>>? setup,
            Action<ParseContext<TInput, TOutput>>? cleanup,
            string name = ""
        )
        {
            _inner = inner;
            _setup = setup;
            _cleanup = cleanup;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public bool Match(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            try
            {
                _setup?.Invoke(new ParseContext<TInput, TOutput>(_inner, state, default));
            }
            catch
            {
                startCp.Rewind();
                return false;
            }

            bool result = false;
            try
            {
                result = _inner.Match(state);
            }
            finally
            {
                _cleanup?.Invoke(new ParseContext<TInput, TOutput>(_inner, state, default));
            }

            return result;
        }

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            try
            {
                _setup?.Invoke(new ParseContext<TInput, TOutput>(_inner, state, default));
            }
            catch (Exception setupException)
            {
                return state.Fail(this, "Setup code threw an exception", new ResultData(setupException));
            }

            Result<TOutput> result = default;
            try
            {
                result = _inner.Parse(state);
            }
            finally
            {
                _cleanup?.Invoke(new ParseContext<TInput, TOutput>(_inner, state, result));
            }

            // If the callbacks consumed any input, be clear about how much we've consumed and who
            // consumed it.
            if (!result.Success)
                startCp.Rewind();
            var totalConsumed = state.Input.Consumed - startCp.Consumed;
            return totalConsumed == result.Consumed
                ? result
                : result with { Consumed = totalConsumed, Parser = this };
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public INamed SetName(string name) => new Parser<TOutput>(_inner, _setup, _cleanup, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TOutput> _inner;
        private readonly Action<MultiParseContext<TInput, TOutput>>? _setup;
        private readonly Action<MultiParseContext<TInput, TOutput>>? _cleanup;

        public MultiParser(
            IMultiParser<TInput, TOutput> inner,
            Action<MultiParseContext<TInput, TOutput>>? setup,
            Action<MultiParseContext<TInput, TOutput>>? cleanup,
            string name = ""
        )
        {
            _inner = inner;
            _setup = setup;
            _cleanup = cleanup;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            int consumedInSetup = 0;
            try
            {
                _setup?.Invoke(new MultiParseContext<TInput, TOutput>(_inner, state, default));
                consumedInSetup = state.Input.Consumed - startCp.Consumed;
            }
            catch (Exception setupException)
            {
                if (consumedInSetup > 0)
                    startCp.Rewind();
                return new MultiResult<TOutput>(_inner, Array.Empty<ResultAlternative<TOutput>>(), new ResultData(setupException));
            }

            MultiResult<TOutput> result = default;
            try
            {
                result = _inner.Parse(state);
            }
            finally
            {
                _cleanup?.Invoke(new MultiParseContext<TInput, TOutput>(_inner, state, result));
            }

            // If the callbacks consumed any input, be clear about how much we've consumed and who
            // consumed it.
            result = result.SelectMany(a => a with { Consumed = a.Consumed + consumedInSetup });
            return consumedInSetup == 0 ? result : result with { Parser = this };
        }

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public INamed SetName(string name) => new MultiParser<TOutput>(_inner, _setup, _cleanup, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
