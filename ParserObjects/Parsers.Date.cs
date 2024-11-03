using System;
using ParserObjects.Internal.Grammars.Data;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// A parser for DateTime format string specifiers. Takes a string like
    /// "dd/MM/yyyy hh:mm:ss.fff" and returns a parser which can parse strings in that format
    /// into DateTimeOffset values.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IParser<char, DateTimeOffset>> DateAndTimeFormat()
        => GetOrCreate("DateTime Format", DateTimeGrammar.CreateDateAndTimeFormatParser);

    /// <summary>
    /// Parse a string in the given format to a DateTimeOffset.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static IParser<char, DateTimeOffset> DateAndTime(string format)
        => GetOrCreate(
            $"DateTime({format})",
            () => DateAndTimeFormat().Parse(format).Value
        );

    /// <summary>
    /// Parse a string in the ISO-8601 format ("YYYY-MM-dd HH:mm:ss.fff") into a DateTimeOffset
    /// structure. Equivalent to DateAndTime("YYYY-MM-dd HH:mm:ss.fff").
    /// </summary>
    /// <returns></returns>
    public static IParser<char, DateTimeOffset> DateAndTimeIso8601()
        => DateAndTime(DateTimeGrammar.Iso8601Format);

    /// <summary>
    /// A parser for Date format string specifiers. Takes a string like
    /// "dd/MM/yyyy" and returns a parser which can parse strings in that format
    /// into DateTime values.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IParser<char, DateTime>> DateFormat()
        => GetOrCreate("Date Format", DateTimeGrammar.CreateDateFormatParser);

    /// <summary>
    /// Parse a Date in the given format to a DateTime.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static IParser<char, DateTime> Date(string format)
        => GetOrCreate(
            $"Date({format})",
            () => DateFormat().Parse(format).Value
        );

    /// <summary>
    /// A parser for Time format string specifiers. Takes a string like
    /// "hh:mm:ss.fff" and returns a parser which can parse strings in that format
    /// into TimeSpan values.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, IParser<char, TimeSpan>> TimeFormat()
        => GetOrCreate("Time Format", DateTimeGrammar.CreateTimeFormatParser);

    /// <summary>
    /// Pares a time in the given format to a TimeSpan.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static IParser<char, TimeSpan> Time(string format)
        => GetOrCreate(
            $"Time({format})",
            () => TimeFormat().Parse(format).Value
        );
}
