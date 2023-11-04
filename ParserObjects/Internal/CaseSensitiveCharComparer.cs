using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal;

public sealed class CaseSensitiveCharComparer : IEqualityComparer<char>
{
    public static CaseSensitiveCharComparer Instance { get; } = new CaseSensitiveCharComparer();

    public bool Equals(char x, char y) => CharMethods.EqualsCaseSensitive(x, y);

    public int GetHashCode([DisallowNull] char obj)
    {
        return obj.GetHashCode();
    }
}
