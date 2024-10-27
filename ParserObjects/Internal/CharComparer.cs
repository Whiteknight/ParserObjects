using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal;

public static class CharComparer
{
    public static CaseInsensitiveComparer CaseInsensitive { get; } = new CaseInsensitiveComparer();

    public static CaseSensitiveComparer CaseSensitive { get; } = new CaseSensitiveComparer();

    public static IEqualityComparer<char> Get(bool caseSensitive = true)
        => caseSensitive
        ? CaseSensitive
        : CaseInsensitive;

    public sealed class CaseInsensitiveComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y) => CharMethods.EqualsCaseInsensitive(x, y);

        public int GetHashCode([DisallowNull] char obj) => char.ToUpper(obj).GetHashCode();
    }

    public sealed class CaseSensitiveComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y) => CharMethods.EqualsCaseSensitive(x, y);

        public int GetHashCode([DisallowNull] char obj) => obj.GetHashCode();
    }
}
