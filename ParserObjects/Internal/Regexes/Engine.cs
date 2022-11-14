using System;
using System.Collections.Generic;
using ParserObjects.Internal.Sequences;
using ParserObjects.Internal.Utility;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

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
        private readonly Stack<(int consumed, int captureIndex)> _consumptions;

        private int _totalConsumptions;

        public BacktrackState(bool isBacktrackable, State state)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            _consumptions = new Stack<(int, int)>();
            _totalConsumptions = 0;
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
                _consumptions.Push((0, -1));
                IsBacktrackable = false;
                _totalConsumptions = 0;
            }
        }

        public (int consumption, int captureIndex) GetNextConsumption()
        {
            if (_consumptions.Count == 0)
                return (0, -1);
            var (consumption, captureIndex) = _consumptions.Pop();
            _totalConsumptions -= consumption;
            return (consumption, captureIndex);
        }

        public int TotalConsumed => _totalConsumptions;

        public bool HasConsumptions => _consumptions.Count > 0;

        public void AddConsumption(int consumed, int captureIndex)
        {
            _consumptions.Push((consumed, captureIndex));
            _totalConsumptions += consumed;
        }
    }

    private sealed class CaptureCollection
    {
        private readonly List<(int group, string value)> _captures;

        public CaptureCollection()
        {
            _captures = new List<(int, string)>();
            CaptureIndex = -1;
        }

        public int CaptureIndex { get; private set; }

        public int AddCapture(int group, string value)
        {
            int currentIndex = CaptureIndex + 1;
            if (_captures.Count > currentIndex)
                _captures[currentIndex] = (group, value);
            else
            {
                _captures.Add((group, value));
                currentIndex = _captures.Count - 1;
            }

            CaptureIndex = currentIndex;
            return CaptureIndex;
        }

        public void ResetCaptureIndex(int captureIndex)
        {
            CaptureIndex = captureIndex >= _captures.Count ? _captures.Count - 1 : captureIndex;
        }

        public IReadOnlyList<(int group, string value)> ToList()
        {
            if (CaptureIndex < 0)
                return Array.Empty<(int, string)>();
            var result = new (int, string)[CaptureIndex + 1];
            for (int i = 0; i <= CaptureIndex; i++)
                result[i] = _captures[i];
            return result;
        }

        public string? GetLatestValueForGroup(int groupNumber)
        {
            for (int i = CaptureIndex; i >= 0; i--)
            {
                if (_captures[i].group == groupNumber)
                    return _captures[i].value;
            }

            return null;
        }
    }

    // Holds information about the current state of the match
    private sealed class RegexContext
    {
        private readonly Stack<State> _queue;
        private readonly Stack<BacktrackState> _backtrackStack;

        // current index into the character buffer
        private int _index;

        public RegexContext(IReadOnlyList<State> states, CaptureCollection captures)
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
            Captures = captures;
        }

        public State CurrentState { get; private set; }

        public int Index => _index;

        public CaptureCollection Captures { get; }

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
                var (consumption, captureIndex) = backtrackState.GetNextConsumption();
                _index -= consumption;
                if (captureIndex >= 0)
                    Captures.ResetCaptureIndex(captureIndex);
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
    /// <param name="maxItems"></param>
    /// <returns></returns>
    public static MatchResult GetMatch(ISequence<char> input, Regex regex, int maxItems = 0)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(regex, nameof(regex));

        var startLocation = input.CurrentLocation;
        var buffer = new SequenceBuffer<char>(input, maxItems);
        var captures = new CaptureCollection();
        var (matches, consumed) = Test(captures, regex.States, buffer);
        if (matches)
        {
            var charArray = buffer.Capture(consumed);
            return new MatchResult(new string(charArray), consumed, startLocation, captures.ToList());
        }

        return new MatchResult($"Match failed at position {consumed}", startLocation);
    }

    private static (bool matches, int length) Test(CaptureCollection captures, IReadOnlyList<State> states, SequenceBuffer<char> buffer)
    {
        var context = new RegexContext(states, captures);

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

    private static void HandleSuccessfulMatch(RegexContext context, BacktrackState backtrackState, int consumed, int captureIndexBeforeMatch)
    {
        backtrackState.AddConsumption(consumed, captureIndexBeforeMatch);
        context.AdvanceIndex(consumed);
    }

    private static bool TestExactlyOne(RegexContext context, SequenceBuffer<char> buffer)
    {
        var captureIndexBeforeMatch = context.Captures.CaptureIndex;
        var (matches, consumed) = MatchStateHere(context, buffer, context.CurrentState, context.Index);
        if (matches)
        {
            var backtrackState = new BacktrackState(false, context.CurrentState);
            context.Push(backtrackState);
            HandleSuccessfulMatch(context, backtrackState, consumed, captureIndexBeforeMatch);
            context.MoveToNextState();
            return true;
        }

        return context.Backtrack();
    }

    private static void TestZeroOrOne(RegexContext context, SequenceBuffer<char> buffer)
    {
        if (buffer.IsPastEnd(context.Index))
        {
            var backtrackState = new BacktrackState(false, context.CurrentState);
            backtrackState.AddZeroConsumed();
            context.Push(backtrackState);
            context.MoveToNextState();
            return;
        }

        var captureIndexBeforeMatch = context.Captures.CaptureIndex;
        var (matches, consumed) = MatchStateHere(context, buffer, context.CurrentState, context.Index);
        if (matches && consumed > 0)
        {
            var backtrackState = new BacktrackState(true, context.CurrentState);
            context.Push(backtrackState);
            HandleSuccessfulMatch(context, backtrackState, consumed, captureIndexBeforeMatch);
            context.MoveToNextState();
            return;
        }

        var fallbackBtState = new BacktrackState(false, context.CurrentState);
        fallbackBtState.AddConsumption(consumed, -1);
        context.Push(fallbackBtState);
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

            var captureIndexBeforeMatch = context.Captures.CaptureIndex;
            var (matches, consumed) = MatchStateHere(context, buffer, context.CurrentState, context.Index);
            if (!matches || consumed == 0)
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            HandleSuccessfulMatch(context, backtrackState, consumed, captureIndexBeforeMatch);
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

            var captureIndexBeforeMatch = context.Captures.CaptureIndex;
            var (matches, consumed) = MatchStateHere(context, buffer, context.CurrentState, context.Index);
            if (!matches || consumed == 0)
            {
                backtrackState.AddZeroConsumed();
                context.Push(backtrackState);
                context.MoveToNextState();
                break;
            }

            HandleSuccessfulMatch(context, backtrackState, consumed, captureIndexBeforeMatch);
            j++;
            if (j >= context.CurrentState.Maximum)
            {
                context.MoveToNextState();
                break;
            }
        }
    }

    private static (bool matches, int length) MatchStateHere(RegexContext context, SequenceBuffer<char> buffer, State state, int index)
    {
        if (buffer.IsPastEnd(index))
        {
            if (state.Type == StateType.EndOfInput)
                return (true, 0);
            return (false, 0);
        }

        if (state.Type == StateType.EndOfInput)
            return (false, 0);

        if (state.Type == StateType.MatchValue)
        {
            var match = state.ValuePredicate?.Invoke(buffer[index]) ?? false;
            return (match, match ? 1 : 0);
        }

        if (state.Type == StateType.CapturingGroup)
        {
            var groupBuffer = buffer.CopyFrom(index);
            var (match, length) = Test(context.Captures, state.Group!, groupBuffer);
            if (!match)
                return (false, 0);

            var value = new string(groupBuffer.Extract(0, length));
            context.Captures.AddCapture(state.GroupNumber, value);
            return (match, length);
        }

        if (state.Type == StateType.NonCapturingCloister)
        {
            var groupBuffer = buffer.CopyFrom(index);
            return Test(context.Captures, state.Group!, groupBuffer);
        }

        if (state.Type == StateType.MatchBackreference)
        {
            var captureValue = context.Captures.GetLatestValueForGroup(state.GroupNumber);
            if (captureValue == null)
                return (false, 0);

            for (int i = 0; i < captureValue.Length; i++)
            {
                if (buffer[index + i] != captureValue[i])
                    return (false, 0);
            }

            return (true, captureValue.Length);
        }

        if (state.Type == StateType.Alternation)
        {
            foreach (var substate in state.Alternations!)
            {
                var (matches, consumed) = Test(context.Captures, substate, buffer.CopyFrom(index));
                if (matches)
                    return (true, consumed);
            }

            return (false, 0);
        }

        throw new RegexException("Unsupported state type during match");
    }
}
