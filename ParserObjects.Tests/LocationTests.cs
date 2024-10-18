namespace ParserObjects.Tests;

public class LocationTests
{
    [Test]
    public void ToString_NoFileName()
    {
        var target = new Location(null, 5, 7);
        target.ToString().Should().Be("Line 5 Column 7");
    }

    [Test]
    public void ToString_FileName()
    {
        var target = new Location("test.txt", 5, 7);
        target.ToString().Should().Be("File test.txt at Line 5 Column 7");
    }
}
