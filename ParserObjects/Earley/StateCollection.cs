using System.Collections.Generic;
using System.Diagnostics;

namespace ParserObjects.Earley
{
    public class StateCollection
    {
        private StateNode _current;

        private readonly Dictionary<int, StateNode> _lookup;

        public StateCollection(ISequenceCheckpoint cp)
        {
            InitialState = new State(0, cp);
            _current = new StateNode(InitialState);
            _lookup = new Dictionary<int, StateNode> { { 0, _current } };
        }

        public State InitialState { get; }

        private class StateNode
        {
            public StateNode(State state)
            {
                Next = null;
                State = state;
            }

            public StateNode? Next { get; set; }

            public State State { get; set; }
        }

        // Get the future State i which is ahead of current State n. If the State i does not
        // exist, create it. Otherwise return it as-is.
        public State GetAhead(int consumed, ISequence input)
        {
            if (consumed <= 0)
                return _current.State;

            var i = _current.State.Number + consumed;
            if (_lookup.ContainsKey(i))
                return _lookup[i].State;

            var state = new State(i, input.Checkpoint());
            var current = _current;

            Debug.Assert(i > current.State.Number, "We shouldn't be going backwards");
            while (current.Next != null && current.Next.State.Number < i)
                current = current.Next;

            var newNode = new StateNode(state)
            {
                State = state,
                Next = current.Next
            };
            current.Next = newNode;
            _lookup.Add(i, newNode);
            return state;
        }

        // Move ahead to the next existing, non-empty state. If there are no more states, return
        // null
        public State? MoveToNext()
        {
            var next = _current.Next;
            if (next == null)
                return null;

            // Break the chain, old states will be referenced by Items if they are needed.
            _current.Next = null;

            // We're done with this state, so we can remove it from the list
            _lookup.Remove(_current.State.Number);

            _current = next;
            return _current.State;
        }
    }
}
