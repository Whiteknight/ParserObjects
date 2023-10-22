using System;
using ParserObjects.Internal.Grammars.Data;

namespace ParserObjects;

public static partial class Parsers
{
    public static IParser<char, DateTime> DateAndTime(string format)
    {
        var formatParser = DateTimeGrammar.CreateFormatParser();
        return formatParser.Parse(format).Value.Named($"date({format})");
    }
}
