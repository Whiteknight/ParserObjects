using System;
using System.Collections.Generic;
using static ParserObjects.Sequences;

namespace ParserObjects.Internal.Parsers;

public sealed record class ComposeParser<TInput, TMiddle, TOutput>(
    IParser<TInput, TMiddle> Lexer,
    IParser<TMiddle, TOutput> Parser,
    Func<ResultFactory<TInput, TMiddle>, Result<TMiddle>>? OnEnd,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IEnumerable<IParser> GetChildren() => new IParser[] { Lexer, Parser };

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public Result<TOutput> Parse(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        var innerState = GetInnerState(state);

        // If the Inner parser fails for whatever reason, it returns an error result, the sequence
        // will throw an InnerParserFailedException, and this parser returns a failure with
        // necessary details.

        // If we provide an OnEnd, that will be used to generate end sentinel values for the inner
        // sequence, which should avoid throwing an error in that case. If we do not provide OnEnd,
        // we will be at the mercy of the inner parser's behavior with regards to end of input.
        // If the inner parser returns an error, it will generate an exception and bail out with
        // an error here. If the inner parser returns a graceful success result with it's own
        // end-of-input value, the outer parser may be able to transform that into something
        // meaningful.

        try
        {
            var outerResult = Parser.Parse(innerState);
            if (!outerResult.Success)
                startCp.Rewind();
            return outerResult;
        }
        catch (InnerParserFailedException ipf)
        {
            startCp.Rewind();
            return state.Fail(this, ipf.ErrorResult.ErrorMessage, new ResultData(ipf.ErrorResult));
        }
    }

    private IParseState<TMiddle> GetInnerState(IParseState<TInput> state)
    {
        var cached = state.Cache.Get<IParseState<TMiddle>>(this, default);
        if (cached.Success)
            return cached.Value;

        var innerSequence = FromParseResult(state.Input, Lexer, getEndSentinel: OnEnd)
            .Select(static r =>
            {
                if (r.Success)
                    return r.Value;
                throw new InnerParserFailedException(r);
            });
        IParseState<TMiddle> innerState = new ParseState<TMiddle>(innerSequence, s => state.Log(this, s));
        state.Cache.Add(this, default, innerState);
        return innerState;
    }

    public INamed SetName(string name) => this with { Name = name };

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        throw new NotImplementedException();
    }

    private class InnerParserFailedException : ControlFlowException
    {
        public InnerParserFailedException(Result<TMiddle> errorResult)
            : base(errorResult.ErrorMessage)
        {
            ErrorResult = errorResult;
        }

        public Result<TMiddle> ErrorResult { get; }
    }
}
