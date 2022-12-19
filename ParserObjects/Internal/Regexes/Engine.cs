using System.Collections.Generic;
using ParserObjects.Internal.Utility;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

/// <summary>
/// Engine to execute regex pattern matching given a Regex and an input sequence.
/// </summary>
public static class Engine
{
    /// <summary>
    /// Attempts to match the given regex pattern on the given input starting at it's current
    /// location. Returns the matched text and any captures and metadata.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public static MatchResult GetMatch(ISequence<char> input, Regex regex)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(regex, nameof(regex));

        var startLocation = input.CurrentLocation;
        var captures = new CaptureCollection();
        var startCheckpoint = input.Checkpoint();
        var matches = Test(captures, regex.States, input);
        if (matches)
        {
            var endCheckpoint = input.Checkpoint();
            var charArray = input.GetBetween(startCheckpoint, endCheckpoint);
            return new MatchResult(new string(charArray), charArray.Length, startLocation, captures.ToList());
        }

        return new MatchResult($"Match failed at position {startCheckpoint.Consumed}", startLocation);
    }

    public static bool TestMatch(ISequence<char> input, Regex regex)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(regex, nameof(regex));

        var captures = new CaptureCollection();
        return Test(captures, regex.States, input);
    }

    private static bool Test(CaptureCollection captures, IReadOnlyList<IState> states, ISequence<char> input)
    {
        var context = new RegexContext(input, states, captures);

        var beforeMatch = input.Checkpoint();
        var currentCheckpoint = beforeMatch;
        while (context.CurrentState is not EndSentinelState)
        {
            switch (context.CurrentState.Quantifier)
            {
                case Quantifier.ExactlyOne:
                    {
                        var ok = TestExactlyOne(context, currentCheckpoint);
                        if (ok)
                            break;
                        beforeMatch.Rewind();
                        return false;
                    }

                case Quantifier.ZeroOrOne:
                    {
                        TestZeroOrOne(context, currentCheckpoint);
                        break;
                    }

                case Quantifier.ZeroOrMore:
                    {
                        TestZeroOrMore(context, currentCheckpoint);
                        break;
                    }

                case Quantifier.Range:
                    {
                        TestRange(context, currentCheckpoint);
                        break;
                    }

                default:
                    throw new RegexException("Unrecognized quantifier");
            }

            currentCheckpoint = input.Checkpoint();
        }

        return true;
    }

    private static bool TestExactlyOne(RegexContext context, SequenceCheckpoint beforeMatch)
    {
        var captureIndexBeforeMatch = context.Captures.CaptureIndex;
        var matches = context.CurrentState.Match(context, beforeMatch, Test);
        if (matches)
        {
            var backtrackState = new BacktrackState(false, context.CurrentState);
            context.Push(backtrackState);
            backtrackState.AddConsumption(beforeMatch, captureIndexBeforeMatch);
            context.MoveToNextState();
            return true;
        }

        // if we can backtrack, the Backtrack() code will rewind the sequence automatically. If we
        // cannot backtrack, the Test() method will rewind for us.
        return context.Backtrack();
    }

    private static void TestZeroOrOne(RegexContext context, SequenceCheckpoint beforeMatch)
    {
        // If we're at the end of input, treat it like 0 matches and return
        if (context.Input.IsAtEnd)
        {
            var backtrackState = new BacktrackState(false, context.CurrentState);
            backtrackState.AddZeroConsumed(beforeMatch, context.Captures.CaptureIndex);
            context.Push(backtrackState);
            context.MoveToNextState();
            return;
        }

        var captureIndexBeforeMatch = context.Captures.CaptureIndex;
        var matches = context.CurrentState.Match(context, beforeMatch, Test);

        // If we match and the match consumes input, setup a backtrack state to go back and try 0
        // instead.
        if (matches && context.Input.Consumed > beforeMatch.Consumed)
        {
            var backtrackState = new BacktrackState(true, context.CurrentState);
            context.Push(backtrackState);
            backtrackState.AddConsumption(beforeMatch, captureIndexBeforeMatch);
            context.MoveToNextState();
            return;
        }

        // Otherwise rewind if we need to
        if (!matches)
            beforeMatch.Rewind();
        var fallbackBtState = new BacktrackState(false, context.CurrentState);
        fallbackBtState.AddZeroConsumed(beforeMatch, captureIndexBeforeMatch);
        context.Push(fallbackBtState);
        context.MoveToNextState();
    }

    private static void TestZeroOrMore(RegexContext context, SequenceCheckpoint beforeMatch)
    {
        var backtrackState = new BacktrackState(true, context.CurrentState);
        var currentCheckpoint = beforeMatch;
        while (true)
        {
            // At end of input, track zero consumed and return
            if (context.Input.IsAtEnd)
            {
                backtrackState.AddZeroConsumed(currentCheckpoint, context.Captures.CaptureIndex);
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            var captureIndexBeforeMatch = context.Captures.CaptureIndex;
            var matches = context.CurrentState.Match(context, currentCheckpoint, Test);

            // If we do not match, or the match consumes 0 input, track zero input so we don't
            // attempt to backtrack here.
            if (!matches || context.Input.Consumed == currentCheckpoint.Consumed)
            {
                if (!matches)
                    currentCheckpoint.Rewind();
                backtrackState.AddZeroConsumed(currentCheckpoint, captureIndexBeforeMatch);
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            // We have a match and it consumed input, so keep track of the consumption.
            backtrackState.AddConsumption(currentCheckpoint, captureIndexBeforeMatch);
            currentCheckpoint = context.Input.Checkpoint();
        }
    }

    private static void TestRange(RegexContext context, SequenceCheckpoint beforeMatch)
    {
        // Ranges are set up to always go from [0-Max) so there is no minimum value.
        // We can bail out at any time.
        var backtrackState = new BacktrackState(true, context.CurrentState);
        int count = 0;
        var currentCheckpoint = beforeMatch;
        while (true)
        {
            // We're out of input, so we're done. Bail out.
            if (context.Input.IsAtEnd)
            {
                backtrackState.AddZeroConsumed(currentCheckpoint, context.Captures.CaptureIndex);
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            var captureIndexBeforeMatch = context.Captures.CaptureIndex;
            var matches = context.CurrentState.Match(context, currentCheckpoint, Test);

            // If it does not match, or the match consumed zero input, track 0 consumptions so
            // we don't backtrack here.
            if (!matches || context.Input.Consumed == currentCheckpoint.Consumed)
            {
                backtrackState.AddZeroConsumed(currentCheckpoint, context.Captures.CaptureIndex);
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            backtrackState.AddConsumption(currentCheckpoint, captureIndexBeforeMatch);

            // Make sure the count is in range. Bail out once we hit the maximum
            count++;
            if (count >= context.CurrentState.Maximum)
            {
                context.MoveToNextState();
                break;
            }

            currentCheckpoint = context.Input.Checkpoint();
        }
    }
}
