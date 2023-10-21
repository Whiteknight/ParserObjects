using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal;

public sealed class CaseInsensitiveCharComparer : IEqualityComparer<char>
{
    public static CaseInsensitiveCharComparer Instance { get; } = new CaseInsensitiveCharComparer();

    public bool Equals(char x, char y)
    {
        // We can really fall into a rabbit-hole here trying to figure out what it means for
        // two characters to be equal in a general case-insensitive way across cultures. For now,
        // this is the least-bad solution I can come up with.
        return char.ToUpper(x) == char.ToUpper(y);
    }

    public int GetHashCode([DisallowNull] char obj)
    {
        return char.ToUpper(obj).GetHashCode();
    }
}
