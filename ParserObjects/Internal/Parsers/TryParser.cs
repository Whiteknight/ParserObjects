using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers which attempt to invoke a parser and catch/handle any exceptions thrown. Built-in
/// parser types are not generally expected to throw exceptions, so this can be used to protect
/// against custom-written parser types.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class TryParser<TInput>
{
    /* We have several parser types for different IParser and IMultiParser interface configurations,
     * all of which delegate to the ParserData<> struct for implementation.
     */

    private readonly struct ParserData<TParser, TResult>
        where TParser : IParser
    {
        public ParserData(TParser parser, Action<Exception>? examine, bool bubble)
        {
            Parser = parser;
            Examine = examine;
            Bubble = bubble;
        }

        public TParser Parser { get; }

        public Action<Exception>? Examine { get; }

        public bool Bubble { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(Func<TParser, IParseState<TInput>, bool> match, IParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            var frame = state.Data.GetCurrentDataFrame();
            try
            {
                return match(Parser, state);
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
                state.Data.PopDataFrame(frame);
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Parse(
            Func<TParser, IParseState<TInput>, TResult> parse,
            IParser tryParser,
            Func<IParser, Exception, SequenceCheckpoint, TResult> createFailure,
            IParseState<TInput> state
        )
        {
            var cp = state.Input.Checkpoint();
            var frame = state.Data.GetCurrentDataFrame();
            try
            {
                return parse(Parser, state);
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
                state.Data.PopDataFrame(frame);
                Examine?.Invoke(ex);
                if (Bubble)
                    throw;
                return createFailure(tryParser, ex, cp);
            }
        }
    }

    public sealed class Parser : IParser<TInput, object>
    {
        private readonly ParserData<IParser<TInput>, Result<object>> _data;

        public Parser(IParser<TInput> inner, Action<Exception>? examine = null, bool bubble = false, string name = "")
        {
            _data = new ParserData<IParser<TInput>, Result<object>>(inner, examine, bubble);
            Name = name;
        }

        private Parser(ParserData<IParser<TInput>, Result<object>> internalParser, string name)
        {
            _data = internalParser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _data.Parser };

        public bool Match(IParseState<TInput> state) => _data.Match(static (p, s) => p.Match(s), state);

        public Result<object> Parse(IParseState<TInput> state) => _data.Parse(
            static (p, s) => p.Parse(s),
            this,
            static (p, ex, _) => Result<object>.Fail(p, ex.Message) with { Data = new ResultData(ex) },
            state
        );

        public INamed SetName(string name) => new Parser(_data, name);

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly ParserData<IParser<TInput, TOutput>, Result<TOutput>> _data;

        public Parser(IParser<TInput, TOutput> inner, Action<Exception>? examine = null, bool bubble = false, string name = "")
        {
            _data = new ParserData<IParser<TInput, TOutput>, Result<TOutput>>(
                inner,
                examine,
                bubble
            );
            Name = name;
        }

        private Parser(ParserData<IParser<TInput, TOutput>, Result<TOutput>> internalParser, string name)
        {
            _data = internalParser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _data.Parser };

        public bool Match(IParseState<TInput> state) => _data.Match(static (p, s) => p.Match(s), state);

        public Result<TOutput> Parse(IParseState<TInput> state) => _data.Parse(
            static (p, s) => p.Parse(s),
            this,
            static (p, ex, _) => Result<TOutput>.Fail(p, ex.Message) with { Data = new ResultData(ex) },
            state
        );

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public INamed SetName(string name) => new Parser<TOutput>(_data, name);

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly ParserData<IMultiParser<TInput, TOutput>, MultiResult<TOutput>> _data;

        public MultiParser(IMultiParser<TInput, TOutput> inner, Action<Exception>? examine = null, bool bubble = false, string name = "")
        {
            _data = new ParserData<IMultiParser<TInput, TOutput>, MultiResult<TOutput>>(
                inner,
                examine,
                bubble
            );
            Name = name;
        }

        private MultiParser(ParserData<IMultiParser<TInput, TOutput>, MultiResult<TOutput>> internalParser, string name)
        {
            _data = internalParser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _data.Parser };

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
            => _data.Parse(
                static (p, s) => p.Parse(s),
                this,
                static (p, ex, _) => new MultiResult<TOutput>(p, Array.Empty<ResultAlternative<TOutput>>(), new ResultData(ex)),
                state
            );

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public INamed SetName(string name) => new MultiParser<TOutput>(_data, name);

        public override string ToString() => DefaultStringifier.ToString("Try", Name, Id);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
