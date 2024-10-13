using System;
using ParserObjects.Internal.Grammars.Data;

namespace ParserObjects;

public static partial class Parsers
{
    public static IParser<char, DateTimeOffset> DateAndTime(string format)
        => DateTimeGrammar.CreateDateAndTimeFormatParser()
            .Parse(format).Value
            .Named($"Date And Time({format})");

    public static IParser<char, DateTime> Date(string format)
        => DateTimeGrammar.CreateDateFormatParser()
            .Parse(format).Value
            .Named($"Date({format})");

    public static IParser<char, TimeSpan> Time(string format)
        => DateTimeGrammar.CreateTimeFormatParser()
            .Parse(format).Value
            .Named($"Time({format})");
}
