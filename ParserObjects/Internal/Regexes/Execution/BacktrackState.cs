using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes.Execution;

public struct BacktrackState
{
    // Keeps track of places in the parse history where backtracking may be possible.

    // Number of characters in a complete match of the current State. For example the regex
    // "(AB)+" could match after 2, 4, or 6 characters, etc. The Engine greedily consumes as
    // many characters as possible for a match, but keeps track of the character counts at each
    // success milestone, so we can backtrack if necessary.
    private readonly Stack<(SequenceCheckpoint? beforeMatch, int captureIndex)> _consumptions;

    public BacktrackState(bool isBacktrackable, IState state)
    {
        IsBacktrackable = isBacktrackable;
        State = state;
        _consumptions = new Stack<(SequenceCheckpoint?, int)>();
    }

    // This point allows backtracking. True if the State supports a variable number of consumed
    // characters.
    public bool IsBacktrackable { get; private set; }

    // The state in the regex which produced this backtrack point
    public IState State { get; }

    // Flag that this state consumed 0 inputs. This means that it could not be backtracked to
    public void AddZeroConsumed(SequenceCheckpoint beforeMatch, int captureIndex)
    {
        if (_consumptions.Count == 0)
        {
            _consumptions.Push((beforeMatch, captureIndex));
            IsBacktrackable = false;
        }
    }

    public (SequenceCheckpoint? beforeMatch, int captureIndex) GetNextConsumption()
        => _consumptions.Count == 0
        ? (null, -1)
        : _consumptions.Pop();

    public readonly bool HasConsumptions => _consumptions.Count > 0;

    public readonly void AddConsumption(SequenceCheckpoint beforeMatch, int captureIndex)
    {
        _consumptions.Push((beforeMatch, captureIndex));
    }
}
