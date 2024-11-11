using System.Collections.Generic;

namespace ParserObjects.Internal;

public static class EnumerableExtensions
{
    public static void AddTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, Dictionary<TKey, TValue> destination)
        where TKey : notnull
    {
        foreach (var (key, value) in source)
            destination.Add(key, value);
    }
}
