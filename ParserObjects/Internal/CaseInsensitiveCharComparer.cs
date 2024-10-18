using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal;

public sealed class CaseInsensitiveCharComparer : IEqualityComparer<char>
{
    public static CaseInsensitiveCharComparer Instance { get; } = new CaseInsensitiveCharComparer();

    public bool Equals(char x, char y) => CharMethods.EqualsCaseInsensitive(x, y);

    public int GetHashCode([DisallowNull] char obj)
    {
        return char.ToUpper(obj).GetHashCode();
    }
}
