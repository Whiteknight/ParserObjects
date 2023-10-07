using System.Collections.Generic;

namespace ParserObjects.Internal;

public static class CharMethods
{
    public static string ConvertToString(IReadOnlyList<char> c)
    {
        return string.Concat(c);
    }
}
