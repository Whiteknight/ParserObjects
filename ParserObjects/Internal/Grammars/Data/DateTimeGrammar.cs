using System;
using System.Globalization;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.Data;

// Takes a format string like "YYYY/MM/dd HH:mm:ss.ms" and returns a parser
// to turn strings of that type into a DateTimeOffset/DateTime/TimeSpan
public static class DateTimeGrammar
{
    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings

    private static int ParseBounded2DigitValue(SequentialState<char> s, int maxValue)
    {
        var firstDigit = s.Parse(Digit());
        var value = firstDigit - '0';
        if (value > (maxValue / 10))
            return value;

        var cp = s.Checkpoint();
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

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _dateParts = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            var yearyyyy = DigitsAsInteger(4, 4).Transform(static y => (IPart)new YearPart(y));

            // MM expects 2 digits, so take two and do the bounds check in MonthPart
            var monthMM = DigitsAsInteger(2, 2).Transform(static m => (IPart)new MonthPart(m));

            var monthM = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 12);
                return (IPart)new MonthPart(value);
            });

            var monthMMM = Trie<int>(static t =>
            {
                for (int i = 0; i < 12; i++)
                    t.Add(DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[i], i + 1);
            }).Transform(static m => (IPart)new MonthPart(m));

            var monthMMMM = Trie<int>(static t =>
            {
                for (int i = 0; i < 12; i++)
                    t.Add(DateTimeFormatInfo.CurrentInfo.MonthNames[i], i + 1);
            }).Transform(static m => (IPart)new MonthPart(m));

            var dayd = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 31);
                return (IPart)new DayPart(value);
            });

            var daydd = DigitsAsInteger(2, 2).Transform(static m => (IPart)new DayPart(m));

            // ddd and dddd specifiers are for day-of-week which is useful for .ToString() but
            // not currently useful for parsing.
            // yy may be useful for 2-digit year parsing, but it would be limited to current century
            return Trie<IParser<char, IPart>>(t => t
                .Add("yyyy", yearyyyy)
                .Add("MMMM", monthMMMM)
                .Add("MMM", monthMMM)
                .Add("MM", monthMM)
                .Add("M", monthM)
                .Add("dd", daydd)
                .Add("d", dayd)
            );
        }
    );

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _timeParts = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            // For each format specifier, when we match it, return an instance of an "internal"
            // parser to handle values of that type.
            var hourHH = DigitsAsInteger(2, 2).Transform(static m => (IPart)new HourPart(m));
            var hourH = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 24);
                return (IPart)new HourPart(value);
            });

            var hourhh = DigitsAsInteger(2, 2).Transform(static m => (IPart)new HourPart(m));
            var hourh = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 12);
                return (IPart)new HourPart(value);
            });

            var minutemm = DigitsAsInteger(2, 2).Transform(static m => (IPart)new MinutePart(m));

            var minutem = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 59);

                return (IPart)new MinutePart(value);
            });

            var secondss = DigitsAsInteger(1, 2).Transform(static m => (IPart)new SecondPart(m));

            var seconds = Sequential(s =>
            {
                var value = ParseBounded2DigitValue(s, 59);

                return (IPart)new SecondPart(value);
            });

            var millisecondfffff = DigitsAsInteger(5, 5)
                .Transform(static m => (IPart)new MillisecondPart(m / 100));
            var millisecondffff = DigitsAsInteger(4, 4)
                .Transform(static m => (IPart)new MillisecondPart(m / 10));
            var millisecondfff = DigitsAsInteger(3, 3)
                .Transform(static m => (IPart)new MillisecondPart(m));
            var millisecondff = DigitsAsInteger(2, 2)
                .Transform(static m => (IPart)new MillisecondPart(m * 10));
            var millisecondf = DigitsAsInteger(1, 1)
                .Transform(static m => (IPart)new MillisecondPart(m * 100));

            return Trie<IParser<char, IPart>>(t => t
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
        }
    );

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _literal = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            // A literal matches any single character which isn't one of the format specifiers.
            // It would be nice if this method could consume mulitiple characters into a string
            // and ignore them all together, but for now this works and isn't too memory-intensive
            // for simple formats.
            var currentTimeSeparator = Match(DateTimeFormatInfo.CurrentInfo.TimeSeparator)
                .Transform(static _ => (IPart)LiteralPart.Instance);
            var currentDateSeparator = Match(DateTimeFormatInfo.CurrentInfo.DateSeparator)
                .Transform(static _ => (IPart)LiteralPart.Instance);
            return First(
                Rule(
                    Match('\\'),
                    Any(),
                    static (_, c) => MatchChar(c).Transform(static _ => (IPart)LiteralPart.Instance)
                ),
                Match(':').Transform(currentTimeSeparator, static (p, _) => p),
                Match('/').Transform(currentDateSeparator, static (p, _) => p),
                Any().Transform(static c => MatchChar(c).Transform(static _ => (IPart)LiteralPart.Instance))
            ).Named("literal");
        }
    );

    public static IParser<char, IParser<char, DateTimeOffset>> CreateDateAndTimeFormatParser()
    {
        var part = First(
            _dateParts.Value,
            _timeParts.Value,
            _literal.Value
        );

        return part
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    foreach (var result in results)
                        dateTime = ((IPart)result).AddTo(dateTime);
                    return new DateTimeOffset(dateTime, TimeSpan.Zero);
                })
            )
            .Named("DateAndTime Format");
    }

    public static IParser<char, IParser<char, DateTime>> CreateDateFormatParser()
    {
        var part = First(
            _dateParts.Value,
            _literal.Value
        );

        return part
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    foreach (var result in results)
                        dateTime = ((IPart)result).AddTo(dateTime);
                    return dateTime;
                })
            )
            .Named("Date Format");
    }

    public static IParser<char, IParser<char, TimeSpan>> CreateTimeFormatParser()
    {
        var part = First(
            _timeParts.Value,
            _literal.Value
        );

        return part
            .List(1)
            .FollowedBy(End())
            .Transform(static parsers => Combine(parsers)
                .Transform(static results =>
                {
                    DateTime dateTime = DateTime.MinValue;
                    foreach (var result in results)
                        dateTime = ((IPart)result).AddTo(dateTime);
                    return dateTime.TimeOfDay;
                })
            )
            .Named("Time Format");
    }

    private interface IPart
    {
        DateTime AddTo(DateTime dt);
    }

    private sealed class YearPart : IPart
    {
        private readonly int _value;

        public YearPart(int value)
        {
            Assert.ArgumentGreaterThanOrEqualTo(value, 1);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(_value, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }
    }

    private sealed class MonthPart : IPart
    {
        private readonly int _value;

        public MonthPart(int value)
        {
            Assert.ArgumentInRange(value, 1, 12);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, _value, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }
    }

    private sealed class DayPart : IPart
    {
        private readonly int _value;

        public DayPart(int value)
        {
            Assert.ArgumentInRange(value, 1, 31);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, _value, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }
    }

    private sealed class HourPart : IPart
    {
        private readonly int _value;

        public HourPart(int value)
        {
            Assert.ArgumentInRange(value, 0, 23);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, _value, dt.Minute, dt.Second, dt.Millisecond);
        }
    }

    private sealed class MinutePart : IPart
    {
        private readonly int _value;

        public MinutePart(int value)
        {
            Assert.ArgumentInRange(value, 0, 59);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, _value, dt.Second, dt.Millisecond);
        }
    }

    private sealed class SecondPart : IPart
    {
        private readonly int _value;

        public SecondPart(int value)
        {
            Assert.ArgumentInRange(value, 0, 59);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, _value, dt.Millisecond);
        }
    }

    private sealed class MillisecondPart : IPart
    {
        private readonly int _value;

        public MillisecondPart(int value)
        {
            Assert.ArgumentInRange(value, 0, 999);
            _value = value;
        }

        public DateTime AddTo(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, _value);
        }
    }

    private sealed class LiteralPart : IPart
    {
        public static LiteralPart Instance { get; } = new LiteralPart();

        public DateTime AddTo(DateTime dt) => dt;
    }
}
