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
                Name = describe;
            var states = new List<List<RegexState>> { new List<RegexState>() };
            regex.BuildUpStates(states);
            if (states.Count != 1)
                throw new RegexException("Invalid regular expression. Too many incomplete groups");
            _states = states[0];
        }

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

        private (bool matches, int length) Test(List<RegexState> states, RegexInputBuffer context)
        {
            var queue = new List<RegexState>(states);
            int i = 0;
            RegexState getNextState()
            {
                if (queue.Count == 0)
                    return null;
                var next = queue[0];
                queue.RemoveAt(0);
                return next;
            }

            var currentState = getNextState();

            void moveToNextState()
            {
                currentState = getNextState();
            }

            var backtrackStack = new Stack<BacktrackState>();
            bool backtrack()
            {
                queue.Insert(0, currentState);
                var couldBacktrack = false;
                while (backtrackStack.Count > 0)
                {
                    var (isBacktrackable, state, consumptions) = backtrackStack.Pop();
                    if (isBacktrackable)
                    {
                        if (consumptions.Count == 0)
                        {
                            queue.Insert(0, state);
                            continue;
                        }
                        var n = consumptions.Pop();
                        i -= n;
                        backtrackStack.Push(new BacktrackState(isBacktrackable, state, consumptions));
                        couldBacktrack = true;
                        break;
                    }
                    queue.Insert(0, state);
                    foreach (var n in consumptions)
                        i -= n;
                }
                if (couldBacktrack)
                    moveToNextState();
                return couldBacktrack;
            }

            while (currentState != null)
            {
                switch (currentState.Quantifier)
                {
                    case RegexQuantifier.ExactlyOne:
                    {
                        var (matches, consumed) = StateMatchesAtCurrentLocation(currentState, context, i);
                        if (!matches)
                        {
                            var indexBeforeBacktracking = i;
                            var couldBacktrack = backtrack();
                            if (!couldBacktrack)
                                return (false, indexBeforeBacktracking);
                            continue;
                        }
                        backtrackStack.Push(new BacktrackState(false, currentState, consumed));
                        i += consumed;
                        moveToNextState();
                        continue;
                    }
                    case RegexQuantifier.ZeroOrOne:
                    {
                        if (context.IsPastEnd(i))
                        {
                            backtrackStack.Push(new BacktrackState(false, currentState, 0));
                            currentState = getNextState();
                            continue;
                        }
                        var (matches, consumed) = StateMatchesAtCurrentLocation(currentState, context, i);
                        backtrackStack.Push(new BacktrackState(matches && consumed > 0, currentState, consumed));
                        i += consumed;
                        moveToNextState();
                        continue;
                    }
                    case RegexQuantifier.ZeroOrMore:
                    {
                        var backtrackState = new BacktrackState(true, currentState);
                        while (true)
                        {
                            if (context.IsPastEnd(i))
                            {
                                backtrackState.AddZeroConsumed();
                                backtrackStack.Push(backtrackState);
                                currentState = getNextState();
                                break;
                            }

                            var (matches, consumed) = StateMatchesAtCurrentLocation(currentState, context, i);
                            if (!matches || consumed == 0)
                            {
                                backtrackState.AddZeroConsumed();
                                backtrackStack.Push(backtrackState);
                                currentState = getNextState();
                                break;
                            }

                            backtrackState.Consumptions.Push(consumed);
                            i += consumed;
                        }
                        continue;
                    }
                    case RegexQuantifier.Range:
                    {
                        var backtrackState = new BacktrackState(true, currentState);
                        int j = 0;
                        while (true)
                        {
                            if (context.IsPastEnd(i))
                            {
                                backtrackState.AddZeroConsumed();
                                backtrackStack.Push(backtrackState);
                                currentState = getNextState();
                                break;
                            }

                            var (matches, consumed) = StateMatchesAtCurrentLocation(currentState, context, i);
                            if (!matches || consumed == 0)
                            {
                                backtrackState.AddZeroConsumed();
                                backtrackStack.Push(backtrackState);
                                currentState = getNextState();
                                break;
                            }

                            backtrackState.Consumptions.Push(consumed);
                            i += consumed;
                            j++;
                            if (j >= currentState.Maximum)
                            {
                                currentState = getNextState();
                                break;
                            }
                        }
                        continue;
                    }

                    default:
                        throw new RegexException("Unrecognized quantifier");
                }
            }

            return (true, i);
        }

        private (bool matches, int length) StateMatchesAtCurrentLocation(RegexState state, RegexInputBuffer context, int i)
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
