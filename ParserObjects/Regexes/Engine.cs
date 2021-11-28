using System.Collections.Generic;
using System.Linq;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects.Regexes;

/// <summary>
/// Engine to execute regex pattern matching given a Regex and an input sequence.
/// </summary>
public static class Engine
{
    private sealed class BacktrackState
    {
        public BacktrackState(bool isBacktrackable, State state)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            Consumptions = new Stack<int>();
        }

        public BacktrackState(bool isBacktrackable, State state, int consumed)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            Consumptions = new Stack<int>();
            Consumptions.Push(consumed);
        }

        public BacktrackState(bool isBacktrackable, State state, Stack<int> consumptions)
        {
            IsBacktrackable = isBacktrackable;
            State = state;
            Consumptions = consumptions;
        }

        public bool IsBacktrackable { get; set; }

        public State State { get; set; }

        public Stack<int> Consumptions { get; }

        public void AddZeroConsumed()
        {
            if (Consumptions.Count == 0)
            {
                Consumptions.Push(0);
                IsBacktrackable = false;
            }
        }
    }

    private sealed class RegexContext
    {
        private readonly List<State> _queue;
        private readonly Stack<BacktrackState> _backtrackStack;

        private int _i;

        public RegexContext(IEnumerable<State> states)
        {
            _queue = new List<State>(states.Where(s => s.Type != StateType.Fence));
            _queue.Add(State.EndSentinel);
            _backtrackStack = new Stack<BacktrackState>();
            _i = 0;
            CurrentState = _queue[0];
            _queue.RemoveAt(0);
        }

        public State CurrentState { get; private set; }

        public int Index => _i;

        public void AdvanceIndex(int i)
        {
            _i += i;
        }

        public void MoveToNextState()
        {
            if (_queue.Count == 0)
            {
                CurrentState = State.EndSentinel;
                return;
            }

            CurrentState = _queue[0];
            _queue.RemoveAt(0);
        }

        public bool Backtrack()
        {
            _queue.Insert(0, CurrentState);
            var couldBacktrack = false;
            while (_backtrackStack.Count > 0)
            {
                var backtrackState = _backtrackStack.Pop();
                if (!backtrackState.IsBacktrackable)
                {
                    _queue.Insert(0, backtrackState.State);
                    foreach (var consumed in backtrackState.Consumptions)
                        _i -= consumed;
                    continue;
                }

                if (backtrackState.Consumptions.Count == 0)
                {
                    _queue.Insert(0, backtrackState.State);
                    continue;
                }

                var n = backtrackState.Consumptions.Pop();
                _i -= n;
                _backtrackStack.Push(new BacktrackState(backtrackState.IsBacktrackable, backtrackState.State, backtrackState.Consumptions));
                couldBacktrack = true;

                break;
            }

            if (couldBacktrack)
                MoveToNextState();
            return couldBacktrack;
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

            backtrackState.Consumptions.Push(consumed);
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

            backtrackState.Consumptions.Push(consumed);
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
