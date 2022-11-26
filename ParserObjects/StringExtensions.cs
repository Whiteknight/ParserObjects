using ParserObjects.Internal.Sequences;

namespace ParserObjects;

public static class StringExtensions
{
    /// <summary>
    /// Wrap the string as a sequence of characters.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ISequence<char> ToCharacterSequence(this string str, SequenceOptions<char> options = default)
        => new StringCharacterSequence(str, options);
}
