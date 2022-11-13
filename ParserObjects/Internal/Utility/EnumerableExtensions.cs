using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Internal.Utility;

public static class EnumerableExtensions
{
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> src)
        => src ?? Enumerable.Empty<T>();
}
