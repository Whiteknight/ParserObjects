using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class TimeTests
{
    [TestCase("HHmmssfff", "103542123")]
    [TestCase("\\HHH\\mmm\\sss\\ffff", "H10m35s42f123")]
    [TestCase("HH:mm:ss.fff", "10:35:42.123")]
    [TestCase("hh:mm:ss.fff", "10:35:42.123")]
    [TestCase("H:m:s.fff", "10:35:42.123")]
    [TestCase("h:m:s.fff", "10:35:42.123")]
    public void AllTwoDigitNumbers(string format, string input)
    {
        var target = Time(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new TimeSpan(0, 10, 35, 42, 123));
    }

    [TestCase("HHmmssfff", "010203004")]
    [TestCase("HH:mm:ss.fff", "01:02:03.004")]
    [TestCase("hh:mm:ss.fff", "01:02:03.004")]
    [TestCase("H:m:s.fff", "1:2:3.004")]
    [TestCase("h:m:s.fff", "1:2:3.004")]
    public void AllOneDigitNumbers(string format, string input)
    {
        var target = Time(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new TimeSpan(0, 1, 2, 3, 4));
    }

    [TestCase("f", "12345", 100)]
    [TestCase("ff", "12345", 120)]
    [TestCase("fff", "12345", 123)]
    [TestCase("ffff", "12345", 123)]
    [TestCase("fffff", "12345", 123)]
    public void Millseconds(string format, string input, int ms)
    {
        var target = Time(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new TimeSpan(0, 0, 0, 0, ms));
    }

    public void Literals()
    {
        var target = Time("test");
        var result = target.Parse("te23t");
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(TimeSpan.FromSeconds(23));
    }
}
