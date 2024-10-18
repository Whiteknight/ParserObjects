using System;

namespace ParserObjects;

/// <summary>
/// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
/// point.
/// </summary>
public readonly record struct SequenceCheckpoint(
    ISequence Sequence,
    int Consumed,
    int Index,
    long StreamPosition,
    Location Location
) : IComparable<SequenceCheckpoint>
{
    /// <summary>
    /// Return the sequence to the state it was when the checkpoint was taken.
    /// </summary>
    public void Rewind()
    {
        Sequence.Rewind(this);
    }

    /// <summary>
    /// Compares to another checkpoint to see which one comes later. If the two checkpoints are
    /// not comparable for some reason, returns 0.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>Negative if this is less than other, 0 if they are equal or uncomparable, positive otherwise.</returns>
    public int CompareTo(SequenceCheckpoint other)
    {
        if (other.Sequence != Sequence)
            return 0;

        return Consumed.CompareTo(other.Consumed);
    }
}
