using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class DateAndTimeTests
{
    [TestCase("yyyyMMddHHmmssfff", "20231021083542123")]
    [TestCase("yyyy-MM-dd HH:mm:ss.fff", "2023-10-21 08:35:42.123")]
    [TestCase("MM/dd/yyyy hh:mm:ss.fff", "10/21/2023 08:35:42.123")]
    [TestCase("dd MMM yyyy H:m:s.fff", "21 Oct 2023 8:35:42.123")]
    [TestCase("dd MMMM yyyy h:m:s.fff", "21 October 2023 8:35:42.123")]
    public void Test(string format, string input)
    {
        var target = DateAndTime(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTimeOffset(2023, 10, 21, 8, 35, 42, 123, TimeSpan.Zero));
    }

    public void Iso8601Test()
    {
        var target = DateAndTimeIso8601();
        var result = target.Parse("2023-10-21 08:35:42.123");
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTimeOffset(2023, 10, 21, 8, 35, 42, 123, TimeSpan.Zero));
    }
}
