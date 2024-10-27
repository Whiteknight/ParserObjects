using System;
using System.Collections.Generic;
using ParserObjects.Internal;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Tries;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    private static readonly Dictionary<char, IParser<char, char>> _matchByChar = new Dictionary<char, IParser<char, char>>();
    private static readonly Dictionary<char, IParser<char, char>> _matchByCharInsensitive = new Dictionary<char, IParser<char, char>>();

    /// <summary>
    /// Invokes the inner parsers using the Match method, in sequence. Returns string of all input
    /// characters which would be part of the match.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<char, string> CaptureString(params IParser<char>[] parsers)
        => parsers == null || parsers.Length == 0
            ? Produce(static () => string.Empty)
            : (IParser<char, string>)new CaptureParser<char, string>(parsers, static (s, start, end) =>
            {
                if (s is ICharSequence charSequence)
                    return charSequence.GetStringBetween(start, end);

                return s.GetBetween(start, end, (object?)null, static (b, _) => new string(b));
            });

    /// <summary>
    /// Convenience method to match a literal sequence of characters and return the
    /// result as a string.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static IParser<char, string> CharacterString(string pattern)
        => MatchChars(pattern);

    /// <summary>
    /// Matches a Letter character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> Letter()
        => GetOrCreate("letter", static () => Match(char.IsLetter));

    /// <summary>
    /// Optimized implementation of First() which returns an input which matches any of the given pattern
    /// strings. Uses a Trie internally to greedily match the longest matching input sequence.
    /// </summary>
    /// <param name="patterns"></param>
    /// <param name="caseInsensitive"></param>
    /// <returns></returns>
    public static IParser<char, string> MatchAny(IEnumerable<string> patterns, bool caseInsensitive = false)
    {
        if (patterns == null)
            return Fail<string>("No possibilities provided so nothing can match");

        var trie = caseInsensitive ? InsertableTrie<char, string>.Create(CharComparer.CaseInsensitive) : InsertableTrie<char, string>.Create();
        foreach (var pattern in patterns)
            trie.Add(pattern, pattern);

        if (trie.Count == 0)
            return Fail<string>("No possibilities provided so nothing can match");

        var readable = ReadableTrie<char, string>.Create(trie);
        return new TrieParser<char, string>(readable);
    }

    /// <summary>
    /// Test whether a char is one of a set of given possibilities. Uses a Contains lookup in your
    /// collection, so your collection should be optimized for fast Contains.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchAny(ICollection<char> possibilities)
        => possibilities == null || possibilities.Count == 0
            ? Fail("No possibilities provided so nothing can match")
            : new MatchPredicateParser<char, ICollection<char>>(possibilities, static (c, p) => p.Contains(c));

    /// <summary>
    /// Return success if the next character matches any characters from the string of possibilities,
    /// failure otherwise.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <param name="caseInsensitive"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchAny(string possibilities, bool caseInsensitive = false)
    {
        if (string.IsNullOrEmpty(possibilities))
            return Fail("No possibilities provided so nothing can match");
        var collection = new HashSet<char>(possibilities, CharComparer.Get(!caseInsensitive));
        return new MatchPredicateParser<char, HashSet<char>>(collection, static (c, s) => s.Contains(c));
    }

    /// <summary>
    /// Test whether a char is not one of a set of given possibilities.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <returns></returns>
    public static IParser<char, char> NotMatchAny(ICollection<char> possibilities)
        => possibilities == null || possibilities.Count == 0
            ? Any()
            : new MatchPredicateParser<char, ICollection<char>>(possibilities, static (c, p) => !p.Contains(c));

    /// <summary>
    /// Return success if the next character does not match any characters from the string. False
    /// if any characters match.
    /// </summary>
    /// <param name="possibilities"></param>
    /// <param name="caseInsensitive"></param>
    /// <returns></returns>
    public static IParser<char, char> NotMatchAny(string possibilities, bool caseInsensitive = false)
        => string.IsNullOrEmpty(possibilities)
            ? Any()
            : new MatchPredicateParser<char, HashSet<char>>(
                new HashSet<char>(possibilities, CharComparer.Get(!caseInsensitive)),
                static (c, s) => !s.Contains(c)
            );

    /// <summary>
    /// Optimized version of Match(char) which caches common instances for reuse. Notice that this
    /// method cannot match the end sentinel, and will fail if the input is at the end. Use
    /// Match() instead if you want to match the end sentinel.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="caseInsensitive"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchChar(char c, bool caseInsensitive = false)
    {
        if (caseInsensitive)
        {
            var realC = char.ToUpper(c);
            if (_matchByCharInsensitive.TryGetValue(realC, out IParser<char, char>? cached))
                return cached;

            var pi = new MatchPredicateParser<char, char>(realC, static (i, r) => char.ToUpper(i) == r, readAtEnd: false);
            _matchByCharInsensitive.Add(realC, pi);
            return pi;
        }

        if (_matchByChar.TryGetValue(c, out IParser<char, char>? value))
            return value;

        var p = new MatchItemParser<char>(c, readAtEnd: false);
        _matchByChar.Add(c, p);
        return p;
    }

    /// <summary>
    /// Get the next character and return it if it satisfies the given predicate. Notice that
    /// this parser cannot match the end sentinel, even if the end sentinel would satisfy the
    /// given predicate. To match the end sentinel, use Match() instead.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<char, char> MatchChar(Func<char, bool> predicate)
        => new MatchPredicateParser<char, Func<char, bool>>(predicate, static (i, p) => p(i), readAtEnd: false);

    /// <summary>
    /// Match the next several characters in sequence against the given string. Return the string
    /// if all characters match. Failure otherwise.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="caseInsensitive"></param>
    /// <returns></returns>
    public static IParser<char, string> MatchChars(string pattern, bool caseInsensitive = false)
        => new MatchStringPatternParser(pattern, caseInsensitive);

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
    public static IParser<char, char> LowerCase()
        => GetOrCreate("lowerCase", static () => Match(char.IsLower));

    /// <summary>
    /// Given a parser which returns an array of characters, change it to a parser which returns
    /// a string instead.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<char, string> Stringify(IParser<char, char[]> p)
        => Transform(p, static c => new string(c));

    /// <summary>
    /// Given a parser which returns a read-only list of characters, change it to a parser which
    /// returns a string instead.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<char, string> Stringify(IParser<char, IReadOnlyList<char>> p)
        => Transform(p, static c => CharMethods.ConvertToString(c));

    /// <summary>
    /// Matches a symbol or punctuation character.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> Symbol()
        => GetOrCreate("symbol", static () => Match(static c => char.IsPunctuation(c) || char.IsSymbol(c)));
}
