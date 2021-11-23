namespace ParserObjects;

public static partial class ParserMethods
{
    /// <summary>
    /// Convenience method to match a literal sequence of characters and return the
    /// result as a string.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static IParser<char, string> CharacterString(string pattern)
        => ParserMethods<char>.Match(pattern).Transform(c => pattern);
}
