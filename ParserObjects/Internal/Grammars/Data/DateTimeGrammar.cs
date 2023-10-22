using System;
using System.Globalization;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.Digits;

namespace ParserObjects.Internal.Grammars.Data;

public static class DateTimeGrammar
{
    // Takes a DateTime format something like "YYYY/MM/dd HH:mm:ss.ms" and returns a parser
    // to turn strings of that type into a DateTime
    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _dateParts = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            var year4Digit = Match("yyyy")
                .Transform(static _ => DigitsAsInteger(4, 4).Transform(static y => (IPart)new YearPart(y)))
                .Named("yyyy");

            var monthNum = Match("MM")
                .Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new MonthPart(m)))
                .Named("MM");

            var monthAbbrev = Match("MMM")
                .Transform(static _ =>
                    Trie<int>(t =>
                    {
                        for (int i = 0; i < 12; i++)
                            t.Add(DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[i], i + 1);
                    })
                    .Transform(static m => (IPart)new MonthPart(m))
                )
                .Named("MMM");

            var monthName = Match("MMMM")
                .Transform(static _ =>
                    Trie<int>(t =>
                    {
                        for (int i = 0; i < 12; i++)
                            t.Add(DateTimeFormatInfo.CurrentInfo.MonthNames[i], i + 1);
                    })
                    .Transform(static m => (IPart)new MonthPart(m))
                )
                .Named("MMMM");

            var day = Match("dd")
                .Transform(static _ =>
                    DigitsAsInteger(2, 2)
                        .Transform(static m => (IPart)new DayPart(m))
                )
                .Named("dd");

            return First(
                year4Digit,
                monthName,
                monthAbbrev,
                monthNum,
                day
            );
        }
    );

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _timeParts = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            var hour24LeadingZero = Match("HH")
                .Transform(static _ =>
                    DigitsAsInteger(2, 2)
                        .Transform(static m => (IPart)new HourPart(m))
                )
                .Named("HH");

            var hour24 = Match("H")
                .Transform(static _ =>
                    DigitsAsInteger(1, 2)
                        .Transform(static m => (IPart)new HourPart(m))
                )
                .Named("H");

            var hour12LeadingZero = Match("hh")
                .Transform(static _ =>
                    DigitsAsInteger(2, 2)
                        .Transform(static m => (IPart)new HourPart(m))
                )
                .Named("hh");

            var hour12 = Match("h")
                .Transform(static _ =>
                    DigitsAsInteger(1, 2)
                        .Transform(static m => (IPart)new HourPart(m))
                )
                .Named("h");

            var minuteLeadingZero = Match("mm")
                .Transform(static _ =>
                    DigitsAsInteger(2, 2)
                        .Transform(static m => (IPart)new MinutePart(m))
                )
                .Named("mm");

            var minute = Match("m")
                .Transform(static _ =>
                    DigitsAsInteger(1, 2)
                        .Transform(static m => (IPart)new MinutePart(m))
                )
                .Named("m");

            var secondLeadingZero = Match("ss")
                .Transform(static _ =>
                    DigitsAsInteger(2, 2)
                        .Transform(static m => (IPart)new SecondPart(m))
                )
                .Named("ss");

            var second = Match("s")
                .Transform(static _ =>
                    DigitsAsInteger(1, 2)
                        .Transform(static m => (IPart)new SecondPart(m))
                )
                .Named("s");

            var millisecond = Match("f")
                .List(1, 4)
                .Transform(static l =>
                    DigitsAsInteger(l.Count, l.Count)
                        .Transform(static m => (IPart)new MillisecondPart(m))
                )
                .Named("f");

            return First(
                hour24LeadingZero,
                hour24,
                hour12LeadingZero,
                hour12,
                minuteLeadingZero,
                minute,
                secondLeadingZero,
                second,
                millisecond
            );
        }
    );

    private static readonly Lazy<IParser<char, IParser<char, IPart>>> _literal = new Lazy<IParser<char, IParser<char, IPart>>>(
        () =>
        {
            var literal = Any()
                .Transform(c =>
                    MatchChar(c)
                        .Transform(_ => (IPart)LiteralPart.Instance)
                )
                .Named("literal");
            return literal;
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
            .Transform(parsers => Combine(parsers)
                .Transform(results =>
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
            .Transform(parsers => Combine(parsers)
                .Transform(results =>
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
            .Transform(parsers => Combine(parsers)
                .Transform(results =>
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
