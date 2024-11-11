using System;

namespace ParserObjects;

/// <summary>
/// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
/// point. Sequence checkpoint should be treated as opaque and immutable. You should not create
/// your own SequenceCheckpoint nor modify the contents of one you receive from a Sequence.
/// </summary>
public readonly record struct SequenceCheckpoint(
    ISequence Sequence,
    int Consumed,
    int Index,
    long StreamPosition,
    SequenceStateType Flags,
    Location Location
) : IComparable<SequenceCheckpoint>
{
    /// <summary>
    /// Return the sequence to the state it was when the checkpoint was taken.
    /// </summary>
    public void Rewind() => Sequence.Rewind(this);

    /// <summary>
    /// Compares to another checkpoint to see which one comes later. If the two checkpoints are
    /// not comparable for some reason, returns 0.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>Negative if this is less than other, 0 if they are equal or uncomparable, positive otherwise.</returns>
    public int CompareTo(SequenceCheckpoint other)
        => other.Sequence == Sequence
        ? Consumed.CompareTo(other.Consumed)
        : 0;
}
