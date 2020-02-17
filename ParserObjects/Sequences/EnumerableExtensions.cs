using System;
using System.Collections.Generic;

namespace ParserObjects.Sequences
{
    public static class EnumerableExtensions
    {
        public static ISequence<T> AsSequence<T>(this IEnumerable<T> enumerable, T endValue = default)
            => new EnumerableSequence<T>(enumerable, endValue);

        public static ISequence<T> AsSequence<T>(this IEnumerable<T> enumerable, Func<T> getEndValue)
            => new EnumerableSequence<T>(enumerable, getEndValue);
    }
}
