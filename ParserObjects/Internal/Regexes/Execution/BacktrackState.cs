using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes.Execution;

// Keeps track of places in the parse history where backtracking may be possible.
public readonly record struct BacktrackState(
    bool IsBacktrackable,

    // The state in the regex which produced this backtrack point
    IState State,

    // Consumptions is the number of characters in a complete match of the current State. For example the regex
    // "(AB)+" could match after 2, 4, or 6 characters, etc. The Engine greedily consumes as
    // many characters as possible for a match, but keeps track of the character counts at each
    // success milestone, so we can backtrack if necessary.
    Stack<(SequenceCheckpoint? BeforeMatch, int CaptureIndex)> Consumptions)
{
    // Flag that this state consumed 0 inputs. This means that it could not be backtracked to
    public BacktrackState AddZeroConsumed(SequenceCheckpoint beforeMatch, int captureIndex)
    {
        if (Consumptions.Count == 0)
        {
            Consumptions.Push((beforeMatch, captureIndex));
            return this with { IsBacktrackable = false };
        }

        return this;
    }

    public readonly (SequenceCheckpoint? BeforeMatch, int CaptureIndex) GetNextConsumption()
        => Consumptions.Count == 0
        ? (null, -1)
        : Consumptions.Pop();

    public readonly bool HasConsumptions => Consumptions.Count > 0;

    public readonly void AddConsumption(SequenceCheckpoint beforeMatch, int captureIndex)
    {
        Consumptions.Push((beforeMatch, captureIndex));
    }
}
