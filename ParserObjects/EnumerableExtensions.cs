using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Sequences;

namespace ParserObjects;

public static class EnumerableExtensions
{
    /// <summary>
    /// Wrap the enumerable as a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="endSentinel">An end value to return when the sequence is exhausted.</param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IEnumerable<T> enumerable, T? endSentinel = default)
        => FromList<T>(enumerable.ToList(), endSentinel);

    public static ISequence<T?> ToSequence<T>(this IReadOnlyList<T> list, T? endValue = default)
        => FromList<T>(list, endValue);
}
