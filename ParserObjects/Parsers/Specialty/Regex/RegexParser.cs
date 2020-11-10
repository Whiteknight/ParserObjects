using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers.Specialty.Regex
{
    public class RegexParser : IParser<char, string>
    {
        private readonly List<RegexState> _states;

        public RegexParser(IRegexNode regex, string describe)
        {
            if (!string.IsNullOrEmpty(describe))
            {
                Name = describe;
                Pattern = describe;
            }
            var states = new List<List<RegexState>> { new List<RegexState>() };
            regex.BuildUpStates(states);
            if (states.Count != 1)
                throw new RegexException("Invalid regular expression. Too many incomplete groups");
            _states = states[0];
        }

        public string Pattern { get; }

        public string Name { get; set; }

        public IParseResult<string> Parse(ISequence<char> t)
        {
            var startLocation = t.CurrentLocation;
            var context = new RegexInputBuffer(t);
            var (matches, consumed) = Test(_states, context);
            if (matches)
            {
                var str = context.Capture(consumed);
                return new SuccessResult<string>(str, startLocation);
            }
            return new FailResult<string>();
        }

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

        // Uses a buffer to represent the _input sequence as an array
        public class RegexInputBuffer
        {
            private readonly ISequence<char> _input;
            private readonly List<char> _buffer;
            private readonly int _offset;

            public RegexInputBuffer(ISequence<char> input)
            {
                _input = input;
                _buffer = new List<char>();
                _offset = 0;
            }

            public RegexInputBuffer(ISequence<char> input, List<char> buffer, int offset)
            {
                _input = input;
                _buffer = buffer;
                _offset = offset;
            }

            public RegexInputBuffer CopyFrom(int i) => new RegexInputBuffer(_input, _buffer, i);

            public string Capture(int i)
            {
                var chars = _buffer.Skip(_offset).Take(i).ToArray();
                var str = new string(chars);
                for (int j = _buffer.Count - 1; j >= i; j--)
                    _input.PutBack(_buffer[j]);
                return str;
            }

            public char this[int index]
            {
                get
                {
                    var realIndex = _offset + index;
                    FillUntil(realIndex);
                    if (realIndex >= _buffer.Count)
                        return '\0';
                    return _buffer[realIndex];
                }
            }

            public bool IsPastEnd(int index)
            {
                var realIndex = _offset + index;
                FillUntil(realIndex);
                if (realIndex >= _buffer.Count)
                    return true;
                return false;
            }

            private void FillUntil(int i)
            {
                if (_buffer.Count > i)
                    return;
                int numToGet = _buffer.Count - i + 1;
                for (var j = 0; j < numToGet; j++)
                {
                    if (_input.IsAtEnd)
                        break;
                    _buffer.Add(_input.GetNext());
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

        private (bool matches, int length) Test(List<RegexState> states, RegexInputBuffer buffer)
        {
            var context = new RegexContext(states);
            context.MoveToNextState();

            while (context.CurrentState != null)
            {
                switch (context.CurrentState.Quantifier)
                {
                    case RegexQuantifier.ExactlyOne:
                    {
                        var indexBeforeBacktracking = context.Index;
                        var ok = TestExactlyOne(context, buffer);
                        if (ok)
                            continue;
                        return (false, indexBeforeBacktracking);
                    }
                    case RegexQuantifier.ZeroOrOne:
                    {
                        TestZeroOrOne(context, buffer);
                        continue;
                    }
                    case RegexQuantifier.ZeroOrMore:
                    {
                        TestZeroOrMore(context, buffer);
                        continue;
                    }
                    case RegexQuantifier.Range:
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

        private bool TestExactlyOne(RegexContext context, RegexInputBuffer buffer)
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

        private void TestZeroOrOne(RegexContext context, RegexInputBuffer buffer)
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

        private void TestZeroOrMore(RegexContext context, RegexInputBuffer buffer)
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

        private void TestRange(RegexContext context, RegexInputBuffer buffer)
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

        private (bool matches, int length) MatchStateHere(RegexState state, RegexInputBuffer context, int i)
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

        public IParseResult<object> ParseUntyped(ISequence<char> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
