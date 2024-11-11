using System.Collections.Generic;
using System.Diagnostics;

namespace ParserObjects.Internal.Earley;

// The StateCollection maintains two pieces of information: a collection of states and a pointer
// to the "current" state. States in the future (ahead of the current state) are kept together
// with a linked list. When the current state advances the linked list is broken and explicit
// references to those states are removed. States which are still referenced by live Items in
// the parse are kept alive, while any that are unreferenced can be safely collected by the GC
// We keep the linked list so .MoveToNext() is O(1), though it can create some O(n) situations
// in .GetAhead()
public sealed class StateCollection
{
    private StateNode _current;

    private readonly Dictionary<int, StateNode> _lookup;

    public StateCollection(SequenceCheckpoint cp)
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
    public State GetAhead(int consumed, SequenceCheckpoint checkpoint)
    {
        if (consumed <= 0)
            return _current.State;

        var i = _current.State.Number + consumed;
        if (_lookup.TryGetValue(i, out var existing))
            return existing.State;

        var state = new State(i, checkpoint);
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
        // This way intermediate states which are no longer referenced by live items can be
        // garbage collected
        _current.Next = null;

        // We're done with this state, so we can remove it from the list. If the state is
        // still referenced by a live Item it will be kept alive. Otherwise the GC will get it.
        _lookup.Remove(_current.State.Number);

        _current = next;
        return _current.State;
    }
}
