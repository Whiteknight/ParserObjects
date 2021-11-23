using ParserObjects.Sequences;

namespace ParserObjects;

public static class StringExtensions
{
    /// <summary>
    /// Wrap the string as a sequence of characters.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="normalizeLineEndings"></param>
    /// <param name="endSentinel"></param>
    /// <returns></returns>
    public static ISequence<char> ToCharacterSequence(this string str, bool normalizeLineEndings = true, char endSentinel = '\0')
        => new StringCharacterSequence(str, normalizeLineEndings: normalizeLineEndings, endSentinel: endSentinel);
}
