using System;
using System.Globalization;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects.Internal.Grammars.Data;

// Takes a format string like "YYYY/MM/dd HH:mm:ss.ms" and returns a parser
// to turn strings of that type into a DateTimeOffset/DateTime/TimeSpan
public static class DateTimeGrammar
{
    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings

    /* A format string like "yyyy MM DD" becomes a list of parsers like:
     *
     *      (yearyyyy, literal, monthMM, literal, daydd)
     *
     * Then when we parse a string like "2024 01 02" we parse the year, the literal space, the
     * month, the literal space and the day.
     *
     * Each format specifier creates a parser that matches some input and returns a Part. Then
     * we take an empty DateTime and add each Part to the DateTime, one after the other. Anything
     * that isn't a known format specified is a "literal", which matches that single character
     * and returns a Part which does nothing. ':' and '/' matches the current time separator and
     * date separator, respectively. The `\` escapes the next character so it isn't treated as a
     * format specifier.
     *
     * The final parser is a Compose() over a list of IParser<char, Part>, and then a .Transform
     * to convert a list of Parts to a DateTime
     */

    public static IParser<char, IParser<char, DateTimeOffset>> CreateDateAndTimeFormatParser()
        => GetOrCreate("DateTime Format", () => First(
                DateParts(),
                TimeParts(),
                Literal()
            )
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    for (int i = 0; i < results.Count; i++)
                        dateTime = results[i].AddTo(dateTime);
                    return new DateTimeOffset(dateTime, TimeSpan.Zero);
                })
            )
        );

    public static IParser<char, IParser<char, DateTime>> CreateDateFormatParser()
        => GetOrCreate("Date Format", () => First(
                DateParts(),
                Literal()
            )
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    for (int i = 0; i < results.Count; i++)
                        dateTime = results[i].AddTo(dateTime);
                    return dateTime;
                })
            )
        );

    public static IParser<char, IParser<char, TimeSpan>> CreateTimeFormatParser()
        => GetOrCreate("Time Format", () => First(
                TimeParts(),
                Literal()
            )
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    for (int i = 0; i < results.Count; i++)
                        dateTime = results[i].AddTo(dateTime);
                    return dateTime.TimeOfDay;
                })
            )
        );

    private static IParser<char, IParser<char, Part>> DateParts()
        => GetOrCreate("DateTime.dateParts", () =>
        {
            var yearyyyy = DigitsAsInteger(4, 4).Transform(static y => new Part(PartType.Year, y));

            // MM expects 2 digits, so take two and do the bounds check in MonthPart
            var monthMM = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 12);
                return new Part(PartType.Month, value);
            });
            var monthM = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 12);
                return new Part(PartType.Month, value);
            });

            var monthMMM = Trie<int>(static t =>
            {
                for (int i = 0; i < 12; i++)
                    t.Add(DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[i], i + 1);
            }).Transform(static m => new Part(PartType.Month, m));

            var monthMMMM = Trie<int>(static t =>
            {
                for (int i = 0; i < 12; i++)
                    t.Add(DateTimeFormatInfo.CurrentInfo.MonthNames[i], i + 1);
            }).Transform(static m => new Part(PartType.Month, m));

            var dayd = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 31);
                return new Part(PartType.Day, value);
            });

            var daydd = DigitsAsInteger(2, 2).Transform(static m => new Part(PartType.Day, m));

            // ddd and dddd specifiers are for day-of-week which is useful for .ToString() but
            // not currently useful for parsing.
            // yy may be useful for 2-digit year parsing, but it would be limited to current century
            return Trie<IParser<char, Part>>(t => t
                .Add("yyyy", yearyyyy)
                .Add("MMMM", monthMMMM)
                .Add("MMM", monthMMM)
                .Add("MM", monthMM)
                .Add("M", monthM)
                .Add("dd", daydd)
                .Add("d", dayd)
            );
        });

    private static IParser<char, IParser<char, Part>> TimeParts()
        => GetOrCreate("DateTime.timeParts", () =>
        {
            // For each format specifier, when we match it, return an instance of an "internal"
            // parser to handle values of that type.
            var hourHH = DigitsAsInteger(2, 2).Transform(static m => new Part(PartType.Hour, m));
            var hourH = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 24);
                return new Part(PartType.Hour, value);
            });

            var hourhh = DigitsAsInteger(2, 2).Transform(static m => new Part(PartType.Hour, m));
            var hourh = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 12);
                return new Part(PartType.Hour, value);
            });

            var minutemm = DigitsAsInteger(2, 2).Transform(static m => new Part(PartType.Minute, m));
            var minutem = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 59);
                return new Part(PartType.Minute, value);
            });

            var secondss = DigitsAsInteger(1, 2).Transform(static m => new Part(PartType.Second, m));
            var seconds = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 59);
                return new Part(PartType.Second, value);
            });

            var millisecondfffff = DigitsAsInteger(5, 5)
                .Transform(static m => new Part(PartType.Millisecond, m / 100));
            var millisecondffff = DigitsAsInteger(4, 4)
                .Transform(static m => new Part(PartType.Millisecond, m / 10));
            var millisecondfff = DigitsAsInteger(3, 3)
                .Transform(static m => new Part(PartType.Millisecond, m));
            var millisecondff = DigitsAsInteger(2, 2)
                .Transform(static m => new Part(PartType.Millisecond, m * 10));
            var millisecondf = DigitsAsInteger(1, 1)
                .Transform(static m => new Part(PartType.Millisecond, m * 100));

            return Trie<IParser<char, Part>>(t => t
                .Add("HH", hourHH)
                .Add("H", hourH)
                .Add("hh", hourhh)
                .Add("h", hourh)
                .Add("m", minutem)
                .Add("mm", minutemm)
                .Add("s", seconds)
                .Add("ss", secondss)
                .Add("f", millisecondf)
                .Add("ff", millisecondff)
                .Add("fff", millisecondfff)
                .Add("ffff", millisecondffff)
                .Add("fffff", millisecondfffff)
            );
        });

    private static IParser<char, IParser<char, Part>> Literal()
        => GetOrCreate("DateTime.Literal", () =>
        {
            // A literal matches any single character which isn't one of the format specifiers.
            // It would be nice if this method could consume mulitiple characters into a string
            // and ignore them all together, but for now this works and isn't too memory-intensive
            // for simple formats.
            var currentTimeSeparator = Match(DateTimeFormatInfo.CurrentInfo.TimeSeparator)
                .Transform(static _ => Part.Literal);
            var currentDateSeparator = Match(DateTimeFormatInfo.CurrentInfo.DateSeparator)
                .Transform(static _ => Part.Literal);
            return First(
                Rule(
                    Match('\\'),
                    Any(),
                    static (_, c) => MatchChar(c).Transform(static _ => Part.Literal)
                ),
                Match(':').Transform(currentTimeSeparator, static (p, _) => p),
                Match('/').Transform(currentDateSeparator, static (p, _) => p),
                Any().Transform(static c => MatchChar(c).Transform(static _ => Part.Literal))
            ).Named("literal");
        });

    private static int ParseBounded2DigitValue(SequentialState<char> s, int maxValue)
    {
        // Must get at least one digit
        var value = s.Parse(Digit()) - '0';

        // If that digit would be too large to be in the 10's place, just return it.
        if (value > (maxValue / 10))
            return value;

        var cp = s.Checkpoint();

        // See if there's a second digit to get. If the second digit is not present or if adding
        // the second digit puts us over the max value, return the first digit only. Otherwise
        // return the whole thing.
        var secondDigit = s.TryParse(Digit());
        if (!secondDigit.Success)
            return value;

        var newValue = (value * 10) + (secondDigit.Value - '0');
        if (newValue > maxValue)
        {
            cp.Rewind();
            return value;
        }

        return newValue;
    }

    private enum PartType
    {
        Literal = 0,
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
        Millisecond
    }

    private readonly record struct Part(PartType Type, int Value)
    {
        public static Part Literal => new Part(PartType.Literal, 0);

        public DateTime AddTo(DateTime dt)
            => Type switch
            {
                PartType.Year => new DateTime(Value, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond),
                PartType.Month => new DateTime(dt.Year, Value, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond),
                PartType.Day => new DateTime(dt.Year, dt.Month, Value, dt.Hour, dt.Minute, dt.Second, dt.Millisecond),
                PartType.Hour => new DateTime(dt.Year, dt.Month, dt.Day, Value, dt.Minute, dt.Second, dt.Millisecond),
                PartType.Minute => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, Value, dt.Second, dt.Millisecond),
                PartType.Second => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, Value, dt.Millisecond),
                PartType.Millisecond => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, Value),
                _ => dt
            };
    }
}
