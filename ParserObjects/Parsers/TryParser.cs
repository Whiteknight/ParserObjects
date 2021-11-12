using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parsers to catch and handle exceptions from user code.
    /// </summary>
    public static class Try
    {
        public static void DefaultExamine(Exception _)
        {
            // This is a default method, intentionally left blank
        }

        /// <summary>
        /// Parser to catch and handle exceptions from user code.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        public class Parser<TInput, TOutput> : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _inner;
            private readonly Action<Exception> _examine;
            private readonly bool _bubble;

            public Parser(IParser<TInput, TOutput> inner, Action<Exception>? examine, bool bubble = false)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                _inner = inner;
                Name = string.Empty;
                _examine = examine ?? DefaultExamine;
                _bubble = bubble;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var cp = state.Input.Checkpoint();
                try
                {
                    return _inner.Parse(state);
                }
                catch (ControlFlowException)
                {
                    // These exceptions are used within the library for non-local control flow, and
                    // should not be caught or modified here.
                    throw;
                }
                catch (Exception e)
                {
                    cp.Rewind();
                    _examine.Invoke(e);
                    if (_bubble)
                        throw;
                    return state.Fail(this, e.Message ?? "Caught unhandled exception");
                }
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        /// <summary>
        /// Parser to catch and handle exceptions from user code.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        public class Parser<TInput> : IParser<TInput>
        {
            private readonly IParser<TInput> _inner;
            private readonly Action<Exception> _examine;
            private readonly bool _bubble;

            public Parser(IParser<TInput> inner, Action<Exception>? examine, bool bubble = false)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                _inner = inner;
                Name = string.Empty;
                _examine = examine ?? DefaultExamine;
                _bubble = bubble;
            }

            public string Name { get; set; }

            public IResult Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var cp = state.Input.Checkpoint();
                try
                {
                    return _inner.Parse(state);
                }
                catch (ControlFlowException)
                {
                    // These exceptions are used within the library for non-local control flow, and
                    // should not be caught or modified here.
                    throw;
                }
                catch (Exception e)
                {
                    cp.Rewind();
                    _examine.Invoke(e);
                    if (_bubble)
                        throw;
                    return state.Fail(this, e.Message ?? "Caught unhandled exception");
                }
            }

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        /// <summary>
        /// Parser to catch and handle exceptions from user code.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        public class MultiParser<TInput, TOutput> : IMultiParser<TInput, TOutput>
        {
            private readonly IMultiParser<TInput, TOutput> _inner;
            private readonly Action<Exception> _examine;
            private readonly bool _bubble;

            public MultiParser(IMultiParser<TInput, TOutput> inner, Action<Exception>? examine, bool bubble = false)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                _inner = inner;
                Name = string.Empty;
                _examine = examine ?? DefaultExamine;
                _bubble = bubble;
            }

            public string Name { get; set; }

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var cp = state.Input.Checkpoint();
                try
                {
                    return _inner.Parse(state);
                }
                catch (ControlFlowException)
                {
                    // These exceptions are used within the library for non-local control flow, and
                    // should not be caught or modified here.
                    throw;
                }
                catch (Exception e)
                {
                    cp.Rewind();
                    _examine.Invoke(e);
                    if (_bubble)
                        throw;
                    return new MultiResult<TOutput>(this, cp.Location, cp, Array.Empty<IResultAlternative<TOutput>>());
                }
            }

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
