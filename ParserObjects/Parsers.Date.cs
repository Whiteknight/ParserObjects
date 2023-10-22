using System;
using ParserObjects.Internal.Grammars.Data;

namespace ParserObjects;

public static partial class Parsers
{
    public static IParser<char, DateTimeOffset> DateAndTime(string format)
    {
        var formatParser = DateTimeGrammar.CreateDateAndTimeFormatParser();
        return formatParser.Parse(format).Value.Named($"Date And Time({format})");
    }

    public static IParser<char, DateTime> Date(string format)
    {
        var formatParser = DateTimeGrammar.CreateDateFormatParser();
        return formatParser.Parse(format).Value.Named($"Date({format})");
    }

    public static IParser<char, TimeSpan> Time(string format)
    {
        var formatParser = DateTimeGrammar.CreateTimeFormatParser();
        return formatParser.Parse(format).Value.Named($"Time({format})");
    }
}
