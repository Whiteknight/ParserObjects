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

    public ISequence Sequence { get; }

    public int Index { get; }

    public long StreamPosition { get; }

    /// <summary>
    /// Return the sequence to the state it was when the checkpoint was taken.
    /// </summary>
    public void Rewind()
    {
        Sequence.Rewind(this);
    }

    public int CompareTo(SequenceCheckpoint other)
    {
        if (other.Sequence != Sequence)
            return 0;

        return Consumed.CompareTo(other.Consumed);
    }
}
