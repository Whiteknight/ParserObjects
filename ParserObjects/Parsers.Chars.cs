﻿using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    private static readonly Dictionary<char, IParser<char, char>> _matchByChar = new Dictionary<char, IParser<char, char>>();

    /// <summary>
    /// Matches a Letter character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> Letter() => _letter.Value;

    private static readonly Lazy<IParser<char, char>> _letter = new Lazy<IParser<char, char>>(
        static () => Match(char.IsLetter).Named("letter")
    );

    /// <summary>
    /// Optimized implementation of First() which returns an input which matches any of the given pattern
    /// strings. Uses a Trie internally to greedily match the longest matching input sequence.
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public static IParser<char, string> MatchAny(IEnumerable<string> patterns)
    {
        var trie = new InsertOnlyTrie<char, string>();
        foreach (var pattern in patterns)
            trie.Add(pattern, pattern);
        return new TrieParser<char, string>(trie);
    }

    /// <summary>
    /// Test whether a char is one of a set of given possibilities.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchAny(ICollection<char> possibilities)
        => new MatchPredicateParser<char>(c => possibilities.Contains(c));

    /// <summary>
    /// Test whether a char is not one of a set of given possibilities.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public static IParser<char, char> NotMatchAny(ICollection<char> possibilities)
        => new MatchPredicateParser<char>(c => !possibilities.Contains(c));

    /// <summary>
    /// Optimized version of Match(char) which caches common instances for reuse.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchChar(char c)
    {
        if (_matchByChar.ContainsKey(c))
            return _matchByChar[c];

        var p = new MatchItemParser<char>(c);
        _matchByChar.Add(c, p);
        return p;
    }

    /// <summary>
    /// Matches a series of consecutive letter characters.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Word() => _word.Value;

    private static readonly Lazy<IParser<char, string>> _word = new Lazy<IParser<char, string>>(
        static () => Letter().ListCharToString(true).Named("word")
    );

    /// <summary>
    /// Matches an upper-case letter character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> UpperCase() => _upperCase.Value;

    private static readonly Lazy<IParser<char, char>> _upperCase = new Lazy<IParser<char, char>>(
        static () => Match(char.IsUpper).Named("upperCase")
    );

    /// <summary>
    /// Matches a lower-case letter character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> LowerCase() => _lowerCase.Value;

    private static readonly Lazy<IParser<char, char>> _lowerCase = new Lazy<IParser<char, char>>(
        static () => Match(char.IsLower).Named("lowerCase")
    );

    public static IParser<char, string> Stringify(IParser<char, char[]> p)
        => Transform(p, static c => new string(c));

    /// <summary>
    /// Matches a symbol or punctuation character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> Symbol() => _symbol.Value;

    private static readonly Lazy<IParser<char, char>> _symbol = new Lazy<IParser<char, char>>(
        static () => Match(static c => char.IsPunctuation(c) || char.IsSymbol(c)).Named("lowerCase")
    );
}
