using ParserObjects.Internal;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class CSharp
    {
        public static IParser<char, TEnum> Enum<TEnum>() =>
            Trie<TEnum>(
                t =>
                {
                    // If the enum is integer-based, we should also be able to parse the integer and
                    // return the Enum.
                    foreach (var name in System.Enum.GetNames(typeof(TEnum)))
                        t.Add(name, (TEnum)System.Enum.Parse(typeof(TEnum), name));
                },
                CaseInsensitiveCharComparer.Instance
            )
            .Named($"Enum({typeof(TEnum).Namespace}.{typeof(TEnum).Name}");
    }
}
