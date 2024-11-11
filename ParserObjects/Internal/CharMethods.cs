using System.Collections.Generic;

namespace ParserObjects.Internal;

public static class CharMethods
{
    public static string ConvertToString(IReadOnlyList<char> c) => string.Concat(c);
}
