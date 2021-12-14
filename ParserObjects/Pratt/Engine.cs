using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Pratt;

// Implementation of the Pratt algorithm. For internal use only, use PrattParser instead.
public sealed class Engine<TInput, TOutput>
{
    // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
    // See https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing

    private readonly IReadOnlyList<IParselet<TInput, TOutput>> _nudableParselets;
    private readonly IReadOnlyList<IParselet<TInput, TOutput>> _ledableParselets;

    public Engine(IReadOnlyList<IParselet<TInput, TOutput>> parselets)
    {
        Assert.ArrayNotNullAndContainsNoNulls(parselets, nameof(parselets));
        _nudableParselets = parselets.Where(p => p.CanNud).ToList();
        _ledableParselets = parselets.Where(p => p.CanLed).ToList();
    }

    public PartialResult<TOutput> TryParse(IParseState<TInput> state, int rbp)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var levelCp = state.Input.Checkpoint();
        try
        {
            return Parse(state, rbp);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
        {
            levelCp.Rewind();
            return new PartialResult<TOutput>(pe.Message, pe.Location ?? state.Input.CurrentLocation);
        }
    }

    private PartialResult<TOutput> Parse(IParseState<TInput> state, int rbp)
    {
        var startLocation = state.Input.CurrentLocation;
        var leftResult = GetLeft(state);
        if (!leftResult.Success)
            return new PartialResult<TOutput>(string.Empty, startLocation);
        var leftToken = leftResult.Value!;
        int consumed = leftResult.Consumed;

        while (true)
        {
            var rightResult = GetRight(state, rbp, leftToken);
            if (!rightResult.Success || rightResult.Value == null)
                break;

            // Set the next left value to be the current combined right value and continue
            // the loop
            consumed += rightResult.Consumed;
            leftToken = rightResult.Value;

            // If we have success, but did not consume any input, we will get into an infinite
            // loop if we don't break. One zero-length suffix rule is the maximum
            if (rightResult.Consumed == 0)
                break;
        }

        return new PartialResult<TOutput>(leftToken.Value, consumed, startLocation);
    }

    private PartialResult<IToken<TOutput>> GetRight(IParseState<TInput> state, int rbp, IToken<TOutput> leftToken)
    {
        var cp = state.Input.Checkpoint();
        foreach (var parselet in _ledableParselets.Where(p => rbp < p.Lbp))
        {
            var (success, token, consumed) = parselet.TryGetNext(state);
            if (!success)
                continue;

            var rightContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp, true, parselet.Name);

            // Transform the IToken into IToken<TOutput> using the LeftDenominator rule and
            // the current left value
            var rightResult = token.LeftDenominator(rightContext, leftToken);
            if (!rightResult.Success)
            {
                cp.Rewind();
                continue;
            }

            return new PartialResult<IToken<TOutput>>(rightResult.Value, consumed + rightContext.Consumed, state.Input.CurrentLocation);
        }

        return new PartialResult<IToken<TOutput>>(string.Empty, state.Input.CurrentLocation);
    }

    private PartialResult<IToken<TOutput>> GetLeft(IParseState<TInput> state)
    {
        var cp = state.Input.Checkpoint();
        foreach (var parselet in _nudableParselets)
        {
            var (success, token, consumed) = parselet.TryGetNext(state);
            if (!success)
                continue;

            var leftContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp, consumed > 0, parselet.Name);

            // Transform the IToken into IToken<TInput> using the NullDenominator rule
            var leftResult = token.NullDenominator(leftContext);
            if (!leftResult.Success)
            {
                cp.Rewind();
                continue;
            }

            consumed += leftContext.Consumed;
            return new PartialResult<IToken<TOutput>>(leftResult.Value, consumed, state.Input.CurrentLocation);
        }

        return new PartialResult<IToken<TOutput>>("No parselets matched and transformed at the current position.", state.Input.CurrentLocation);
    }
}
