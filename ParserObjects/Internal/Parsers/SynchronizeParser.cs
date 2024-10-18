using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Panic mode handler. Attempt the parse and return immediately if successful.
/// On error, discard input tokens until a known-good state is reached and
/// continue the attempt from the new location. Returns failure if the first
/// attempt does not succeed, but includes information about all subsequent
/// attempts.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Attempt"></param>
/// <param name="DiscardUntil"></param>
/// <param name="Name"></param>
public sealed record SynchronizeParser<TInput, TOutput>(
    IParser<TInput, TOutput> Attempt,
    Func<TInput, bool> DiscardUntil,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        var result = Attempt.Parse(state);
        if (result.Success)
            return result;

        var allErrors = new List<IResult>
        {
            result
        };

        // Result failed. Enter a loop to discard tokens and try again.
        while (!state.Input.IsAtEnd)
        {
            // Outer loop: Discard a bunch of tokens and then attempt the parse
            // again from a "good" location. Keep doing this until we have a success
            // or we run out of input

            DiscardUntilConditionMet(state);
            if (state.Input.IsAtEnd)
                break;

            result = Attempt.Parse(state);
            if (result.Success)
                break;
            allErrors.Add(result);
        }

        // At this point we have panic'd so we're always going to return a failure.
        // Append all the data necessary so the caller can examine what's going on.
        var data = new List<object>
        {
            new ErrorList(allErrors)
        };
        if (result.Success)
            data.Add(result);

        return state.Fail(this, "One or more errors occured. Call IResult<T>.TryGetData<ErrorList>() for more details", new ResultData(data));
    }

    private void DiscardUntilConditionMet(IParseState<TInput> state)
    {
        while (!state.Input.IsAtEnd)
        {
            // Inner loop: Discard a token. If the DiscardUntil condition is
            // met, or we run out of inputs, stop discarding.
            var discard = state.Input.GetNext();
            if (DiscardUntil(discard))
                break;
        }
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state) => Attempt.Match(state);

    public IEnumerable<IParser> GetChildren() => new[] { Attempt };

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }
}
