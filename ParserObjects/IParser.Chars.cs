namespace ParserObjects;

public static class ParserCharsExtensions
{
    public static IParser<char, string> Stringify(this IParser<char, char[]> p)
        => Parsers.Stringify(p);
}
