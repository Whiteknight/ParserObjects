using System;
using System.Collections.Generic;

namespace ParserObjects.Sequences
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Wrap the enumerable as a sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="endValue">An end value to return when the sequence is exhausted.</param>
        /// <returns></returns>
        public static ISequence<T> ToSequence<T>(this IEnumerable<T> enumerable, T endValue = default)
            => new EnumerableSequence<T>(enumerable, endValue);

        /// <summary>
        /// Wrap the enumerable as a sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="getEndValue">Get a value to return when the sequence is exhausted.</param>
        /// <returns></returns>
        public static ISequence<T> ToSequence<T>(this IEnumerable<T> enumerable, Func<T> getEndValue)
            => new EnumerableSequence<T>(enumerable, getEndValue);
    }
}
