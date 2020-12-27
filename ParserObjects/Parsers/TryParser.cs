using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class TryParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;
        private readonly Action<Exception>? _examine;
        private readonly bool _bubble;

        public TryParser(IParser<TInput, TOutput> inner, Action<Exception>? examine, bool bubble = false)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
            Name = string.Empty;
            _examine = examine ?? DefaultExamine;
            _bubble = bubble;
        }

        public string Name { get; set; }

        public static void DefaultExamine(Exception e)
        {
            // This is a default method, intentionally left blank
        }

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
                _examine?.Invoke(e);
                if (_bubble)
                    throw;
                return state.Fail(this, e.Message ?? "Caught unhandled exception");
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class TryParser<TInput> : IParser<TInput>
    {
        private readonly IParser<TInput> _inner;
        private readonly Action<Exception>? _examine;
        private readonly bool _bubble;

        public TryParser(IParser<TInput> inner, Action<Exception>? examine, bool bubble = false)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
            Name = string.Empty;
            _examine = examine ?? DefaultExamine;
            _bubble = bubble;
        }

        public string Name { get; set; }

        public static void DefaultExamine(Exception e)
        {
            // This is a default method, intentionally left blank
        }

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
                _examine?.Invoke(e);
                if (_bubble)
                    throw;
                return state.Fail(this, e.Message ?? "Caught unhandled exception");
            }
        }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
