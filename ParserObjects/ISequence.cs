using System;
using ParserObjects.Internal.Sequences;

namespace ParserObjects;

/// <summary>
/// An input stream with metadata.
/// </summary>
public interface ISequence
{
    /// <summary>
    /// Gets the approximate location from the source data where the current input item was located, if
    /// available.
    /// </summary>
    Location CurrentLocation { get; }

    /// <summary>
    /// Gets a value indicating whether the sequence is at the end and no more values may be
    /// retrieved. False if the sequence is exhausted and no more values are available.
    /// </summary>
    bool IsAtEnd { get; }

    /// <summary>
    /// Take a snapshot of the state of the sequence, which can be returned to later if the
    /// sequence needs to be rewound.
    /// </summary>
    /// <returns></returns>
    SequenceCheckpoint Checkpoint();

    /// <summary>
    /// Returns true if this checkpoint was created by this sequence. False otherwise.
    /// </summary>
    /// <param name="checkpoint"></param>
    /// <returns></returns>
    bool Owns(SequenceCheckpoint checkpoint);

    /// <summary>
    /// Rewinds the sequence to the location pointed to by the checkpoint. If the checkpoint is not
    /// owned by this sequence, the method will do nothing.
    /// </summary>
    /// <param name="checkpoint"></param>
    void Rewind(SequenceCheckpoint checkpoint);

    /// <summary>
    /// Gets a count of the total number of input items that have been consumed from this
    /// sequence so far.
    /// </summary>
    int Consumed { get; }

    /// <summary>
    /// Get a snapshot of current statistics for the sequence. Not all sequences implement all
    /// statistics values.
    /// </summary>
    /// <returns></returns>
    SequenceStatistics GetStatistics();

    /// <summary>
    /// Reset this sequence back to it's initial position. Will not reset statistics.
    /// </summary>
    void Reset();
}

/// <summary>
/// An input sequence of items. Similar to IEnumerable/IEnumerator but with the ability to rewind and
/// put back items which are not needed.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISequence<out T> : ISequence
{
    /// <summary>
    /// Get the next value from the sequence or a default value if the sequence is at the end, and
    /// increments the location.
    /// </summary>
    /// <returns></returns>
    T GetNext();

    /// <summary>
    /// Gets the next value off the sequence but does not advance the location.
    /// </summary>
    /// <returns></returns>
    T Peek();

    /// <summary>
    /// Get an array of all input values between the two checkpoints. If start is greater than end,
    /// or if one of these checkpoints is not owned by this sequence, returns an empty array.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    T[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end);
}

/// <summary>
/// Sequence type for character-based sequences which may return a string in some operations.
/// </summary>
public interface ICharSequence : ISequence<char>
{
    /// <summary>
    /// Return a string containing all remaining characters from the current string position until
    /// the end of input.
    /// </summary>
    /// <returns></returns>
    string GetRemainder();

    /// <summary>
    /// Return the characters between the two checkpoints as a string.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end);
}

public static class SequenceExtensions
{
    /// <summary>
    /// Transform a sequence of one type into a sequence of another type by applying a transformation
    /// function to every element.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="input"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static ISequence<TOutput> Select<TInput, TOutput>(this ISequence<TInput> input, Func<TInput, TOutput> map)
        => new MapSequence<TInput, TOutput>(input, map);

    /// <summary>
    /// Filter elements in a sequence to only return items which match a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static ISequence<T> Where<T>(this ISequence<T> input, Func<T, bool> predicate)
        => new FilterSequence<T>(input, predicate);
}
