﻿using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class CSharp
    {
        public static IParser<char, TEnum> Enum<TEnum>()
        {
            // Would like to make this case insensitive
            var p = Trie<TEnum>(t =>
            {
                foreach (var name in System.Enum.GetNames(typeof(TEnum)))
                    t.Add(name, (TEnum)System.Enum.Parse(typeof(TEnum), name));
            });

            // If the enum is integer-based, we should also be able to parse the integer and
            // return the Enum.

            return p.Named($"Enum({typeof(TEnum).Namespace}.{typeof(TEnum).Name}");
        }
    }
}
