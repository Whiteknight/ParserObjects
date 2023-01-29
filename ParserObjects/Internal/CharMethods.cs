using System.Collections.Generic;

namespace ParserObjects.Internal;

public static class CharMethods
{
    public static string ConvertToString(IReadOnlyList<char> c)
    {
        // I don't normally like to do this kind of stuff, but this method is pretty important
        // for performance of many parsers, and string.Create() doesn't exist in netstandard2.0
        // The crux of the problem is that many places have an IReadOnlyList<char> but string
        // constructor only takes a char[]. So we need to allocate a new array here, which is
        // costly.

#if NET5_0_OR_GREATER
        return string.Create(c.Count, c, static (buffer, src) =>
        {
            for (int i = 0; i < src.Count; i++)
                buffer[i] = src[i];
        });
#else
        return new string(System.Linq.Enumerable.ToArray(c));
#endif
    }
}
