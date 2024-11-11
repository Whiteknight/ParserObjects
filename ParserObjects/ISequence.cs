using System;
using System.Runtime.CompilerServices;
using ParserObjects.Internal;
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

    SequenceStateType Flags { get; }

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

public delegate TResult MapSequenceSpan<T, TData, TResult>(ReadOnlySpan<T> span, TData data);

/// <summary>
/// An input sequence of items. Similar to IEnumerable/IEnumerator but with the ability to rewind and
/// put back items which are not needed.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISequence<T> : ISequence
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
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="data"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<T, TData, TResult> map);

    public T[] GetArrayBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        => GetBetween(start, end, (object?)null, static (b, _) =>
        {
            var buffer = new T[b.Length];
            b.CopyTo(buffer.AsSpan());
            return buffer;
        });
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
    public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        => GetBetween(start, end, (object?)null, static (b, _) => new string(b));
}

[Flags]
public enum SequenceStateType
{
    None = 0,
    StartOfInput = 1,
    EndOfInput = 2,
    StartOfLine = 4
}

public static class SequenceExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SequenceStateType With(this SequenceStateType source, SequenceStateType toAdd)
        => source | toAdd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SequenceStateType Without(this SequenceStateType source, SequenceStateType toRemove)
        => source & ~toRemove;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(this SequenceStateType source, SequenceStateType test)
        => (source & test) == test;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SequenceStateType Only(this SequenceStateType source, SequenceStateType test)
        => source & test;

    /// <summary>
    /// Transform a sequence of one type into a sequence of another type by applying a transformation
    /// function to every element.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="input"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static ISequence<TOutput> Select<TInput, TOutput>(
        this ISequence<TInput> input,
        Func<TInput, TOutput> map
    ) => new MapSequence<TInput, TOutput>(input, map);

    /// <summary>
    /// Filter elements in a sequence to only return items which match a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static ISequence<T> Where<T>(this ISequence<T> input, Func<T, bool> predicate)
        => new FilterSequence<T>(input, predicate);

    public static T[] GetNext<T>(this ISequence<T> input, int count)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotLessThanOrEqualToZero(count);
        var result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = input.GetNext();
        return result;
    }

    public static string GetString(this ICharSequence input, int count)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotLessThanOrEqualToZero(count);
        var result = new char[count];
        int i = 0;
        for (; !input.IsAtEnd && i < count; i++)
            result[i] = input.GetNext();
        return new string(result, 0, i);
    }

    public static T[] Peek<T>(this ISequence<T> input, int count)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotLessThanOrEqualToZero(count);
        var result = new T[count];
        var cp = input.Checkpoint();
        for (int i = 0; i < count; i++)
            result[i] = input.GetNext();
        cp.Rewind();
        return result;
    }

    public static string PeekString(this ICharSequence input, int count)
    {
        Assert.ArgumentNotNull(input);
        Assert.ArgumentNotLessThanOrEqualToZero(count);
        var result = new char[count];
        var cp = input.Checkpoint();
        int i = 0;
        for (; !input.IsAtEnd && i < count; i++)
            result[i] = input.GetNext();
        cp.Rewind();
        return new string(result, 0, i);
    }
}
