using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Sequences;

namespace ParserObjects;

public static class EnumerableExtensions
{
    /// <summary>
    /// Read the enumerable to a list and then wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="endSentinel">An end value to return when the sequence is exhausted.</param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IEnumerable<T> enumerable, T? endSentinel = default)
        => FromList<T>(enumerable.ToList(), endSentinel);

    /// <summary>
    /// Wrap the list in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="endValue"></param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IReadOnlyList<T> list, T? endValue = default)
        => FromList<T>(list, endValue);
}
