using static ParserObjects.Sequences;

namespace ParserObjects;

public static class StringExtensions
{
    /// <summary>
    /// Wrap the string as a sequence of characters.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ICharSequence ToCharacterSequence(
        this string str,
        SequenceOptions<char> options = default
    ) => FromString(str, options);
}
