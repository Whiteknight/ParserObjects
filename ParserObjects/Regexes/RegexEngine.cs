using System.Collections.Generic;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects.Regexes
{
    /// <summary>
    /// Engine to execute regex pattern matching given a Regex and an input sequence.
    /// </summary>
    public class RegexEngine
    {
        private class BacktrackState
        {
            public BacktrackState(bool isBacktrackable, RegexState state)
            {
                IsBacktrackable = isBacktrackable;
                State = state;
                Consumptions = new Stack<int>();
            }

            public BacktrackState(bool isBacktrackable, RegexState state, int consumed)
            {
                IsBacktrackable = isBacktrackable;
                State = state;
                Consumptions = new Stack<int>();
                Consumptions.Push(consumed);
            }

            public BacktrackState(bool isBacktrackable, RegexState state, Stack<int> consumptions)
            {
                IsBacktrackable = isBacktrackable;
                State = state;
                Consumptions = consumptions;
            }

            public bool IsBacktrackable { get; set; }

            public RegexState State { get; set; }

            public Stack<int> Consumptions { get; }

            public void Deconstruct(out bool isBacktrackable, out RegexState state, out Stack<int> consumptions)
            {
                isBacktrackable = IsBacktrackable;
                state = State;
                consumptions = Consumptions;
            }

            public void AddZeroConsumed()
            {
                if (Consumptions.Count == 0)
                {
                    Consumptions.Push(0);
                    IsBacktrackable = false;
                }
            }
        }

        private class RegexContext
        {
            private readonly List<RegexState> _queue;
            private readonly Stack<BacktrackState> _backtrackStack;

            private int _i;

            public RegexContext(IEnumerable<RegexState> states)
            {
                _queue = new List<RegexState>(states);
                _backtrackStack = new Stack<BacktrackState>();
                _i = 0;
            }

            public RegexState CurrentState { get; private set; }

            public int Index => _i;

            public void AdvanceIndex(int i)
            {
                _i += i;
            }

            public void MoveToNextState()
            {
                if (_queue.Count == 0)
                {
                    CurrentState = null;
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
                    var (isBacktrackable, state, consumptions) = _backtrackStack.Pop();
                    if (isBacktrackable)
                    {
                        if (consumptions.Count == 0)
                        {
                            _queue.Insert(0, state);
                            continue;
                        }

                        var n = consumptions.Pop();
                        _i -= n;
                        _backtrackStack.Push(new BacktrackState(isBacktrackable, state, consumptions));
                        couldBacktrack = true;
                        break;
                    }

                    _queue.Insert(0, state);
                    foreach (var n in consumptions)
                        _i -= n;
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
        public (bool matches, string value, int consumed) GetMatch(ISequence<char> input, Regex regex)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            Assert.ArgumentNotNull(regex, nameof(regex));

            var context = new SequenceBuffer<char>(input);
            var (matches, consumed) = Test(regex.States, context);
            if (matches)
            {
                var charArray = context.Capture(consumed);
                return (true, new string(charArray), consumed);
            }

            return (false, null, consumed);
        }

        private (bool matches, int length) Test(IReadOnlyList<RegexState> states, SequenceBuffer<char> buffer)
        {
            var context = new RegexContext(states);
            context.MoveToNextState();

            while (context.CurrentState != null)
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

        private bool TestExactlyOne(RegexContext context, SequenceBuffer<char> buffer)
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

        private void TestZeroOrOne(RegexContext context, SequenceBuffer<char> buffer)
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

        private void TestZeroOrMore(RegexContext context, SequenceBuffer<char> buffer)
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

        private void TestRange(RegexContext context, SequenceBuffer<char> buffer)
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

        private (bool matches, int length) MatchStateHere(RegexState state, SequenceBuffer<char> context, int i)
        {
            if (context.IsPastEnd(i))
            {
                if (state.Type == RegexStateType.EndOfInput)
                    return (true, 0);
                return (false, 0);
            }

            if (state.Type == RegexStateType.EndOfInput)
                return (false, 0);
            if (state.Type == RegexStateType.MatchValue)
            {
                var match = state.ValuePredicate?.Invoke(context[i]) ?? false;
                return (match, match ? 1 : 0);
            }

            if (state.Type == RegexStateType.Group)
                return Test(state.Group, context.CopyFrom(i));
            if (state.Type == RegexStateType.Alternation)
            {
                foreach (var substate in state.Alternations)
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
}
