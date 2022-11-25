using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

// Holds information about the current state of the match
public sealed class RegexContext
{
    private readonly Stack<IState> _queue;
    private readonly Stack<BacktrackState> _backtrackStack;

    public RegexContext(ISequence<char> input, IReadOnlyList<IState> states, CaptureCollection captures)
    {
        Input = input;
        _queue = new Stack<IState>(states.Count + 1);
        _queue.Push(State.EndSentinel);
        for (int i = states.Count - 1; i >= 0; i--)
        {
            var state = states[i];
            if (state is FenceState)
                continue;
            _queue.Push(state);
        }

        _backtrackStack = new Stack<BacktrackState>(states.Count);

        CurrentState = _queue.Pop();
        Captures = captures;
    }

    public ISequence<char> Input { get; }
    public IState CurrentState { get; private set; }

    public CaptureCollection Captures { get; }

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
            var (beforeMatch, captureIndex) = backtrackState.GetNextConsumption();
            beforeMatch?.Rewind();
            if (captureIndex >= 0)
                Captures.ResetCaptureIndex(captureIndex);
            _backtrackStack?.Push(backtrackState);
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
