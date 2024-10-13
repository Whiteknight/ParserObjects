using System.Collections.Generic;

namespace ParserObjects.Internal;

public static class CharMethods
{
    public static string ConvertToString(IReadOnlyList<char> c) => string.Concat(c);

    public static bool EqualsCaseSensitive(char a, char b) => a == b;

    public static bool EqualsCaseInsensitive(char a, char b) => char.ToUpper(a) == char.ToUpper(b);
}
