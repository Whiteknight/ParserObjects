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
    /* This is very similar to the Examine parser, except in Examine we explicitly do not wrap
     * user callbacks in try/catch because we *expect* the user to use those callbacks for readonly
     * examination and not to make modifications. We don't enforce it, but it is on the user to make
     * sure they use the tool correctly.
     *
     * Here in the Context() parser we have access to all the parse state and are expected to be
     * making modifications. This is why we are much more cautious about try/catch and error handlers
     */

    private readonly struct InternalParser<TParser>
        where TParser : IParser
    {
        private readonly Action<IParseState<TInput>>? _setup;
        private readonly Action<IParseState<TInput>>? _cleanup;

        public InternalParser(
            TParser parser,
            Action<IParseState<TInput>>? setup,
            Action<IParseState<TInput>>? cleanup
        )
        {
            Parser = parser;
            _setup = setup;
            _cleanup = cleanup;
        }

        public TParser Parser { get; }

        public TResult Execute<TResult>(
            IParser parent,
            IParseState<TInput> state,
            Func<IParser, TParser, IParseState<TInput>, (bool success, TResult result)> execute,
            Func<IParser, IParseState<TInput>, SequenceCheckpoint, Exception, TResult> onSetupFail
        )
        {
            // If setup fails, we catch it and return a nice failure message.
            // If the Inner parser throws we attempt to invoke Cleanup, then allow the exception to bubble up.
            // The user might use a Try() or something and deal with it elsewhere.
            // If the cleanup throws an exception, we allow it to bubble up (without cleanup, the parse
            // may be in an invalid state).

            var startCp = state.Input.Checkpoint();

            try
            {
                _setup?.Invoke(state);
            }
            catch (Exception setupException)
            {
                return onSetupFail(parent, state, startCp, setupException);
            }

            bool success = false;
            TResult? result = default;
            try
            {
                (success, result) = execute(parent, Parser, state);
            }
            finally
            {
                _cleanup?.Invoke(state);
            }

            if (!success)
                startCp.Rewind();
            return result;
        }
    }

    public sealed class Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly InternalParser<IParser<TInput, TOutput>> _internal;

        public Parser(
            IParser<TInput, TOutput> inner,
            Action<IParseState<TInput>>? setup,
            Action<IParseState<TInput>>? cleanup,
            string name = ""
        )
        {
            _internal = new InternalParser<IParser<TInput, TOutput>>(inner, setup, cleanup);
            Name = name;
        }

        private Parser(InternalParser<IParser<TInput, TOutput>> internalData, string name)
        {
            _internal = internalData;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public bool Match(IParseState<TInput> state)
            => _internal.Execute(
                this,
                state,
                static (_, p, s) =>
                {
                    bool matched = p.Match(s);
                    return (matched, matched);
                },
                static (_, _, _, _) => false
            );

        public Result<TOutput> Parse(IParseState<TInput> state)
            => _internal.Execute(
                this,
                state,
                static (_, p, s) =>
                {
                    var result = p.Parse(s);
                    return (result.Success, result);
                },
                static (ctx, s, _, ex) => s.Fail<TInput, TOutput>(ctx, "Setup code threw an exception", new ResultData(ex))
            );

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public INamed SetName(string name) => new Parser<TOutput>(_internal, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly InternalParser<IMultiParser<TInput, TOutput>> _internal;

        public MultiParser(
            IMultiParser<TInput, TOutput> inner,
            Action<IParseState<TInput>>? setup,
            Action<IParseState<TInput>>? cleanup,
            string name = ""
        )
        {
            _internal = new InternalParser<IMultiParser<TInput, TOutput>>(inner, setup, cleanup);
            Name = name;
        }

        private MultiParser(InternalParser<IMultiParser<TInput, TOutput>> internalData, string name)
        {
            _internal = internalData;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IMultResult<TOutput> Parse(IParseState<TInput> state)
            => _internal.Execute(
                this,
                state,
                static (_, p, s) =>
                {
                    var result = p.Parse(s);
                    return (result.Success, result);
                },
                static (ctx, _, cp, ex) => new MultResult<TOutput>(ctx, cp, Array.Empty<IResultAlternative<TOutput>>(), new ResultData(ex))
            );

        IMultResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _internal.Parser };

        public INamed SetName(string name) => new MultiParser<TOutput>(_internal, name);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
