using System;

namespace ParserObjects;

/// <summary>
/// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
/// point.
/// </summary>
public interface ISequenceCheckpoint : IComparable
{
    /// <summary>
    /// Return the sequence to the state it was when the checkpoint was taken.
    /// </summary>
    void Rewind();

    /// <summary>
    /// Gets the number of items consumed between the start of input and the current location.
    /// </summary>
    int Consumed { get; }

    /// <summary>
    /// Gets the current location of the input sequence.
    /// </summary>
    Location Location { get; }
}
