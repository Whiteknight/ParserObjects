using System;

namespace ParserObjects;

/// <summary>
/// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
/// point.
/// </summary>
public struct SequenceCheckpoint : IComparable<SequenceCheckpoint>
{
    public SequenceCheckpoint(ISequence sequence, int consumed, int index, long streamPosition, Location location)
    {
        Sequence = sequence;
        Consumed = consumed;
        Index = index;
        StreamPosition = streamPosition;
        Location = location;
    }

    /// <summary>
    /// Gets the number of items consumed between the start of input and the current location.
    /// </summary>
    public int Consumed { get; }

    /// <summary>
    /// Gets the current location of the input sequence.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// Gets the sequence which owns this checkpoint.
    /// </summary>
    public ISequence Sequence { get; }

    /// <summary>
    /// Gets an index value used internally by the sequence to reset itself.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets a position in the stream which the sequence uses internally to reset itself.
    /// </summary>
    public long StreamPosition { get; }

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
    /// <returns></returns>
    public int CompareTo(SequenceCheckpoint other)
    {
        if (other.Sequence != Sequence)
            return 0;

        return Consumed.CompareTo(other.Consumed);
    }
}
