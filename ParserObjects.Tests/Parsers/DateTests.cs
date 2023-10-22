using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class DateTests
{
    [TestCase("yyyyMMdd", "20231021")]
    [TestCase("yyyy-MM-dd", "2023-10-21")]
    [TestCase("MM/dd/yyyy", "10/21/2023")]
    [TestCase("dd MMM yyyy", "21 Oct 2023")]
    [TestCase("dd MMMM yyyy", "21 October 2023")]
    public void Test(string format, string input)
    {
        var target = Date(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTime(2023, 10, 21));
    }
}
