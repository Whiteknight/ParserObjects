using System.Diagnostics.Contracts;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class DateTests
{
    [TestCase("yyyyMMdd", "20231021")]
    [TestCase("\\yyyyy\\MMM\\ddd", "y2023M10d21")]
    [TestCase("yyyy-MM-dd", "2023-10-21")]
    [TestCase("yyyy-M-dd", "2023-10-21")]
    [TestCase("MM/dd/yyyy", "10/21/2023")]
    [TestCase("dd MMM yyyy", "21 Oct 2023")]
    [TestCase("dd MMMM yyyy", "21 October 2023")]
    public void AllTwoDigitNumbers(string format, string input)
        => Test(format, input, 2023, 10, 21);

    [TestCase("yyyyMd", "202318", 2023, 1, 8)]
    public void Test(string format, string input, int year, int month, int day)
    {
        var target = Date(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTime(year, month, day));
    }

    [TestCase("yyyyMd", "20230507")]
    [TestCase("yyyyMd", "202357")]
    [TestCase("yyyy-M-d", "2023-5-7")]
    [TestCase("M/d/yyyy", "5/7/2023")]
    public void AllOneDigitNumbers(string format, string input)
    {
        var target = Date(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTime(2023, 5, 7));
    }
}
