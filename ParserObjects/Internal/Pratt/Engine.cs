using System.Collections.Generic;
using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

// Implementation of the Pratt algorithm. For internal use only, use PrattParser instead.
public sealed class Engine<TInput, TOutput>
{
    // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
    // See https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing

    private readonly IParselet<TInput, TOutput>[] _nudableParselets;
    private readonly int _numNudableParselets;
    private readonly IParselet<TInput, TOutput>[] _ledableParselets;
    private readonly int _numLedableParselets;

    public Engine(IReadOnlyList<IParselet<TInput, TOutput>> parselets)
    {
        Assert.ArrayNotNullAndContainsNoNulls(parselets);
        _nudableParselets = new IParselet<TInput, TOutput>[parselets.Count];
        _ledableParselets = new IParselet<TInput, TOutput>[parselets.Count];
        int numNudable = 0;
        int numLedable = 0;
        for (int i = 0; i < parselets.Count; i++)
        {
            var parselet = parselets[i];
            if (parselet.CanNud)
                _nudableParselets[numNudable++] = parselet;
            if (parselet.CanLed)
                _ledableParselets[numLedable++] = parselet;
        }

        _numLedableParselets = numLedable;
        _numNudableParselets = numNudable;
    }

    // TryParse is the main entry point and recursive re-entry point, and also the jump destination
    // for ParseException when the Severity==Level
    public PartialResult<TOutput> TryParse(IParseState<TInput> state, int rbp)
        => TryParse(state, rbp, new ParseControl());

    public PartialResult<TOutput> TryParse(IParseState<TInput> state, int rbp, ParseControl parseControl)
    {
        Assert.ArgumentNotNull(state);
        var levelCp = state.Input.Checkpoint();
        try
        {
            return Parse(state, rbp, parseControl);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
        {
            levelCp.Rewind();
            return new PartialResult<TOutput>(pe.Message);
        }
    }

    // Main function for performing the parse. Gets a left value and then gets right values until
    // there are no more matches or completion is signalled.
    private PartialResult<TOutput> Parse(IParseState<TInput> state, int rbp, ParseControl parseControl)
    {
        var leftResult = GetLeft(state, parseControl);
        if (!leftResult.Success)
            return new PartialResult<TOutput>(string.Empty);

        if (parseControl.IsComplete)
            return new PartialResult<TOutput>(leftResult.Value!.Value, leftResult.Consumed);

        var leftToken = leftResult.Value!;
        int consumed = leftResult.Consumed;

        while (!parseControl.IsComplete)
        {
            var rightResult = GetRight(state, rbp, leftToken, parseControl);
            if (!rightResult.Success || rightResult.Value.Value == null)
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

        return new PartialResult<TOutput>(leftToken.Value, consumed);
    }

    // Get the next "LED" right value by testing all available parselets in definition order
    private PartialResult<ValueToken<TOutput>> GetRight(IParseState<TInput> state, int minBp, ValueToken<TOutput> leftToken, ParseControl parseControl)
    {
        for (int i = 0; i < _numLedableParselets; i++)
        {
            var parselet = _ledableParselets[i];
            if (minBp >= parselet.Lbp)
                continue;

            var (success, token, consumed) = parselet.TryGetNextLed(state, this, parseControl, leftToken);
            if (!success)
            {
                // If the parse failed we want to keep trying other parselets UNLESS the parse is
                // marked complete.
                if (parseControl.IsComplete)
                    return new PartialResult<ValueToken<TOutput>>("A match was not found at the current position but the parse was marked complete.");
                continue;
            }

            return new PartialResult<ValueToken<TOutput>>(token, consumed);
        }

        return new PartialResult<ValueToken<TOutput>>(string.Empty);
    }

    // Get the next "NUD" left value by testing all available parselets in definition order
    private PartialResult<ValueToken<TOutput>> GetLeft(IParseState<TInput> state, ParseControl parseControl)
    {
        for (int i = 0; i < _numNudableParselets; i++)
        {
            var parselet = _nudableParselets[i];

            var (success, token, consumed) = parselet.TryGetNextNud(state, this, parseControl);
            if (!success)
            {
                // If we failed we want to keep trying other parselets UNLESS the parse is marked
                // complete.
                if (parseControl.IsComplete)
                    return new PartialResult<ValueToken<TOutput>>("A match was not found at the current position but the parse was marked complete.");
                continue;
            }

            return new PartialResult<ValueToken<TOutput>>(token, consumed);
        }

        return new PartialResult<ValueToken<TOutput>>("No parselets matched and transformed at the current position.");
    }
}

// This class exists because we need reference semantics on a flag for marking the parse complete,
// but we do not want the Engine to have mutable data.
public class ParseControl
{
    public bool IsComplete { get; set; }
}
