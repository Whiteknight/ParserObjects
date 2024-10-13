using System.Collections.Generic;
using static ParserObjects.Parsers;

namespace ParserObjects.Internal.Grammars.C;

public static class Constants
{
    public static readonly IReadOnlyDictionary<char, string> EscapableStringChars = new Dictionary<char, string>
    {
        { 'a', "\a" },
        { 'b', "\b" },
        { 'f', "\f" },
        { 'n', "\n" },
        { 'r', "\r" },
        { 't', "\t" },
        { 'v', "\v" },
        { '\\', "\\" },
        { '?', "?" },
        { '"', "\"" },
    };

    public static readonly IParser<char, IReadOnlyList<char>> HexCode = HexadecimalDigit().List(2, 4);
    public static readonly IParser<char, IReadOnlyList<char>> LowUnicodeCode = HexadecimalDigit().List(4, 4);
    public static readonly IParser<char, IReadOnlyList<char>> HighUnicodeCode = HexadecimalDigit().List(8, 8);
    public static readonly IParser<char, string> HexCodeString = HexadecimalDigit().ListCharToString(2, 4);
    public static readonly IParser<char, string> LowUnicodeString = HexadecimalDigit().ListCharToString(4, 4);
    public static readonly IParser<char, string> HighUnicodeString = HexadecimalDigit().ListCharToString(8, 8);
}
