using System.Collections.Generic;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects.Regexes;

/// <summary>
/// Engine to execute regex pattern matching given a Regex and an input sequence.
/// </summary>
public static class Engine
{
    // Keeps track of places in the parse history where backtracking may be possible.
    private sealed class BacktrackState
    {
        // Number of characters in a complete match of the current State. For example the regex
        // "(AB)+" could match after 2, 4, or 6 characters, etc. The Engine greedily consumes as
        // many characters as possible for a match, but keeps track of the character counts at each
        // success milestone, so we can backtrack if necessary.
        private readonly Stack<int> _consumptions;

        private int _totalConsumptions;

        public BacktrackState(bool isBacktrackable, State state)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            _consumptions = new Stack<int>();
            _totalConsumptions = 0;
        }

        public BacktrackState(bool isBacktrackable, State state, int consumed)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            _consumptions = new Stack<int>();
            _consumptions.Push(consumed);
            _totalConsumptions = consumed;
        }

        // This point allows backtracking. True if the State supports a variable number of consumed
        // characters.
        public bool IsBacktrackable { get; private set; }

        // The state in the regex which produced this backtrack point
        public State State { get; }

        // Flag that this state consumed 0 inputs. This means that it could not be backtracked to
        public void AddZeroConsumed()
        {
            if (_consumptions.Count == 0)
            {
                _consumptions.Push(0);
                IsBacktrackable = false;
                _totalConsumptions = 0;
            }
        }

        public int GetNextConsumption()
        {
            if (_consumptions.Count == 0)
                return 0;
            var consumption = _consumptions.Pop();
            _totalConsumptions -= consumption;
            return consumption;
        }

        public int TotalConsumed => _totalConsumptions;

        public bool HasConsumptions => _consumptions.Count > 0;

        public void AddConsumption(int consumed)
        {
            _consumptions.Push(consumed);
            _totalConsumptions += consumed;
        }
    }

    // Holds information about the current state of the match
    private sealed class RegexContext
    {
        private readonly Stack<State> _queue;
        private readonly Stack<BacktrackState> _backtrackStack;

        // current index into the character buffer
        private int _index;

        public RegexContext(IReadOnlyList<State> states)
        {
            _queue = new Stack<State>(states.Count + 1);
            _queue.Push(State.EndSentinel);
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state.Type == StateType.Fence)
                    continue;
                _queue.Push(state);
            }

            _backtrackStack = new Stack<BacktrackState>(states.Count);
            _index = 0;
            CurrentState = _queue.Pop();
        }

        public State CurrentState { get; private set; }

        public int Index => _index;

        public void AdvanceIndex(int numCharacters)
        {
            _index += numCharacters;
        }

        // The previous state matched successfully, so advance to the next state
        public void MoveToNextState()
        {
            if (_queue.Count == 0)
            {
                CurrentState = State.EndSentinel;
                return;
            }

            CurrentState = _queue.Pop();
        }

        // Attempt a backtrack when the CurrentState fails. We look through all the items in the
        // backtrack stack, adding States back to the queue and rewinding the input by the size of
        // backtracked consumption counts.
        public bool Backtrack()
        {
            // Add the CurrentState back to the queue
            _queue.Push(CurrentState);
            while (_backtrackStack.Count > 0)
            {
                // Look for a backtrackable state.
                // If the state is not backtrackable, add the state to the queue and continue
                var backtrackState = _backtrackStack.Pop();
                if (!backtrackState.IsBacktrackable)
                {
                    _queue.Push(backtrackState.State);
                    _index -= backtrackState.TotalConsumed;
                    continue;
                }

                // If the state has no consumptions, add it to the queue and continue
                if (!backtrackState.HasConsumptions)
                {
                    _queue.Push(backtrackState.State);
                    continue;
                }

                // Pull 1 consumption off the current backtrackState, that is going to be the place
                // where we attempt to continue the match. Push that state back onto the stack in
                // case we need to try again with the remaining consumptions.
                var consumption = backtrackState.GetNextConsumption();
                _index -= consumption;
                _backtrackStack.Push(backtrackState);
                CurrentState = _queue.Pop();
                return true;
            }

            return false;
        }

        public void Push(BacktrackState backtrackState)
        {
            _backtrackStack.Push(backtrackState);
        }
    }

    /// <summary>
    /// Attempts to match the given regex pattern on the given input starting at it's current
    /// location.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public static PartialResult<string> GetMatch(ISequence<char> input, Regex regex)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(regex, nameof(regex));

        var startLocation = input.CurrentLocation;
        var buffer = new SequenceBuffer<char>(input);
        var (matches, consumed) = Test(regex.States, buffer);
        if (matches)
        {
            var charArray = buffer.Capture(consumed);
            return new PartialResult<string>(new string(charArray), consumed, startLocation);
        }

        return new PartialResult<string>($"Match failed at position {consumed}", startLocation);
    }

    private static (bool matches, int length) Test(IReadOnlyList<State> states, SequenceBuffer<char> buffer)
    {
        var context = new RegexContext(states);

        while (context.CurrentState.Type != StateType.EndSentinel)
        {
            switch (context.CurrentState.Quantifier)
            {
                case Quantifier.ExactlyOne:
                    {
                        var indexBeforeBacktracking = context.Index;
                        var ok = TestExactlyOne(context, buffer);
                        if (ok)
                            continue;
                        return (false, indexBeforeBacktracking);
                    }

                case Quantifier.ZeroOrOne:
                    {
                        TestZeroOrOne(context, buffer);
                        continue;
                    }

                case Quantifier.ZeroOrMore:
                    {
                        TestZeroOrMore(context, buffer);
                        continue;
                    }

                case Quantifier.Range:
                    {
                        TestRange(context, buffer);
                        continue;
                    }

                default:
                    throw new RegexException("Unrecognized quantifier");
            }
        }

        return (true, context.Index);
    }

    private static bool TestExactlyOne(RegexContext context, SequenceBuffer<char> buffer)
    {
        var (matches, consumed) = MatchStateHere(context.CurrentState, buffer, context.Index);
        if (matches)
        {
            context.Push(new BacktrackState(false, context.CurrentState, consumed));
            context.AdvanceIndex(consumed);
            context.MoveToNextState();
            return true;
        }

        var couldBacktrack = context.Backtrack();
        if (couldBacktrack)
            return true;
        return false;
    }

    private static void TestZeroOrOne(RegexContext context, SequenceBuffer<char> buffer)
    {
        if (buffer.IsPastEnd(context.Index))
        {
            context.Push(new BacktrackState(false, context.CurrentState, 0));
            context.MoveToNextState();
            return;
        }

        var (matches, consumed) = MatchStateHere(context.CurrentState, buffer, context.Index);
        context.Push(new BacktrackState(matches && consumed > 0, context.CurrentState, consumed));
        context.AdvanceIndex(consumed);
        context.MoveToNextState();
    }

    private static void TestZeroOrMore(RegexContext context, SequenceBuffer<char> buffer)
    {
        var backtrackState = new BacktrackState(true, context.CurrentState);
        while (true)
        {
            if (buffer.IsPastEnd(context.Index))
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            var (matches, consumed) = MatchStateHere(context.CurrentState, buffer, context.Index);
            if (!matches || consumed == 0)
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            backtrackState.AddConsumption(consumed);
            context.AdvanceIndex(consumed);
        }
    }

    private static void TestRange(RegexContext context, SequenceBuffer<char> buffer)
    {
        var backtrackState = new BacktrackState(true, context.CurrentState);
        int j = 0;
        while (true)
        {
            if (buffer.IsPastEnd(context.Index))
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            var (matches, consumed) = MatchStateHere(context.CurrentState, buffer, context.Index);
            if (!matches || consumed == 0)
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            backtrackState.AddConsumption(consumed);
            context.AdvanceIndex(consumed);
            j++;
            if (j >= context.CurrentState.Maximum)
            {
                context.MoveToNextState();
                break;
            }
        }
    }

    private static (bool matches, int length) MatchStateHere(State state, SequenceBuffer<char> context, int i)
    {
        if (context.IsPastEnd(i))
        {
            if (state.Type == StateType.EndOfInput)
                return (true, 0);
            return (false, 0);
        }

        if (state.Type == StateType.EndOfInput)
            return (false, 0);

        if (state.Type == StateType.MatchValue)
        {
            var match = state.ValuePredicate?.Invoke(context[i]) ?? false;
            return (match, match ? 1 : 0);
        }

        if (state.Type == StateType.Group)
            return Test(state.Group!, context.CopyFrom(i));

        if (state.Type == StateType.Alternation)
        {
            foreach (var substate in state.Alternations!)
            {
                var (matches, consumed) = Test(substate, context.CopyFrom(i));
                if (matches)
                    return (true, consumed);
            }

            return (false, 0);
        }

        throw new RegexException("Unsupported state type during match");
    }
}
