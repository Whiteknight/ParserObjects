﻿using System;
using ParserObjects.Sequences;

namespace ParserObjects
{
    /// <summary>
    /// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
    /// point.
    /// </summary>
    public interface ISequenceCheckpoint
    {
        /// <summary>
        /// Return the sequence to the state it was when the checkpoint was taken.
        /// </summary>
        void Rewind();
    }

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
        ISequenceCheckpoint Checkpoint();

        /// <summary>
        /// Gets a count of the total number of input items that have been consumed from this
        /// sequence so far.
        /// </summary>
        int Consumed { get; }
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
    }

    public static class SequenceExtensions
    {
        /// <summary>
        /// Create a buffer from the given sequence. A buffer allows random-order accessing of
        /// input items using an array-like interface. Buffers require special cleanup to make
        /// sure unused items are returned to the sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SequenceBuffer<T> CreateBuffer<T>(this ISequence<T> input)
            => new SequenceBuffer<T>(input);

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
}
