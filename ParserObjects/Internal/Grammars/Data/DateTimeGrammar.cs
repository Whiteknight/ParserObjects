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
    public static IParser<char, IParser<char, DateTime>> CreateFormatParser()
    {
        var year4Digit = Match("YYYY").Transform(static _ => DigitsAsInteger(4, 4).Transform(static y => (IPart)new YearPart(y)));

        var monthNum = Match("MM").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new MonthPart(m % 12)));
        var monthAbbrev = Match("MMM").Transform(static _ => Trie<int>(t =>
        {
            for (int i = 0; i < 12; i++)
                t.Add(DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[i], i + 1);
        }).Transform(static m => (IPart)new MonthPart(m)));
        var monthName = Match("MMMM").Transform(static _ => Trie<int>(t =>
        {
            for (int i = 0; i < 12; i++)
                t.Add(DateTimeFormatInfo.CurrentInfo.MonthNames[i], i + 1);
        }).Transform(static m => (IPart)new MonthPart(m)));

        var day = Match("dd").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new DayPart(m % 31)));

        var hour24LeadingZero = Match("HH").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new HourPart(m % 24)));
        var hour24 = Match("H").Transform(static _ => DigitsAsInteger(1, 2).Transform(static m => (IPart)new HourPart(m % 24)));
        var hour12LeadingZero = Match("hh").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new HourPart(m % 12)));
        var hour12 = Match("h").Transform(static _ => DigitsAsInteger(1, 2).Transform(static m => (IPart)new HourPart(m % 12)));

        var minuteLeadingZero = Match("mm").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new MinutePart(m % 60)));
        var minute = Match("m").Transform(static _ => DigitsAsInteger(1, 2).Transform(static m => (IPart)new MinutePart(m % 60)));

        var secondLeadingZero = Match("ss").Transform(static _ => DigitsAsInteger(2, 2).Transform(static m => (IPart)new SecondPart(m % 60)));
        var second = Match("s").Transform(static _ => DigitsAsInteger(1, 2).Transform(static m => (IPart)new SecondPart(m % 60)));

        var millisecond = Match("f").List(1, 4).Transform(static l => DigitsAsInteger(l.Count, l.Count).Transform(static m => (IPart)new MillisecondPart(m)));

        var literal = Any().Transform(c => MatchChar(c).Transform(_ => (IPart)LiteralPart.Instance));

        var part = First(
            year4Digit,
            monthName,
            monthAbbrev,
            monthNum,
            day,
            hour24LeadingZero,
            hour24,
            hour12LeadingZero,
            hour12,
            minuteLeadingZero,
            minute,
            millisecond,
            secondLeadingZero,
            second,
            literal
        );

        return part.List(1).Transform(parsers => Combine(parsers).Transform(results =>
        {
            DateTime dateTime = DateTime.MinValue;
            foreach (var result in results)
                dateTime = ((IPart)result).AddTo(dateTime);
            return dateTime;
        }));
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
