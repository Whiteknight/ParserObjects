using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class TimeTests
{
    [TestCase("HHmmssfff", "083542123")]
    [TestCase("HH:mm:ss.fff", "08:35:42.123")]
    [TestCase("hh:mm:ss.fff", "08:35:42.123")]
    [TestCase("H:m:s.fff", "8:35:42.123")]
    [TestCase("h:m:s.fff", "8:35:42.123")]
    public void Test(string format, string input)
    {
        var target = Time(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new TimeSpan(0, 8, 35, 42, 123));
    }
}
