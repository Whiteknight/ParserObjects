using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Internal.Utility;

public static class CharMethods
{
    public static string ConvertToString(IReadOnlyList<char> c)
    {
        // I don't normally like to do this kind of stuff, but this method is pretty important
        // for performance of many parsers, and string.Create() doesn't exist in netstandard2.0

#if NET5_0_OR_GREATER
            return string.Create(c.Count, c, static (buffer, src) =>
            {
                for (int i = 0; i < src.Count; i++)
                    buffer[i] = src[i];
            });
#else
        return new string(c.ToArray());
#endif
    }
}
