﻿using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class DateTests
{
    [TestCase("YYYYMMddHHmmssfff", "20231021083542123")]
    [TestCase("YYYY-MM-dd HH:mm:ss.fff", "2023-10-21 08:35:42.123")]
    [TestCase("MM/dd/YYYY hh:mm:ss.fff", "10/21/2023 08:35:42.123")]
    [TestCase("dd MMM YYYY H:m:s.fff", "21 Oct 2023 8:35:42.123")]
    [TestCase("dd MMMM YYYY h:m:s.fff", "21 October 2023 8:35:42.123")]
    public void Test(string format, string input)
    {
        var target = Date(format);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var dt = result.Value;
        dt.Should().Be(new DateTime(2023, 10, 21, 8, 35, 42, 123));
    }
}
