using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects;

public static class EnumerableExtensions
{
    /// <summary>
    /// Wrap the enumerable as a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="endValue">An end value to return when the sequence is exhausted.</param>
    /// <returns></returns>
    public static ISequence<T?> ToSequence<T>(this IEnumerable<T> enumerable, T? endValue = default)
        => new ListSequence<T?>(enumerable, endValue);

    public static ISequence<T?> ToSequence<T>(this IReadOnlyList<T> list, T? endValue = default)
        => new ListSequence<T?>(list, endValue);
}
