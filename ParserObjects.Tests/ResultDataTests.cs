namespace ParserObjects.Tests;

public class ResultDataTests
{
    [Test]
    public void One_OfType()
    {
        var target = new ResultData("test");
        var result = target.OfType<string>();
        result.Value.Should().Be("test");
    }

    [Test]
    public void One_OfType_WrongType()
    {
        var target = new ResultData("test");
        var result = target.OfType<int>();
        result.Success.Should().BeFalse();
    }

    [Test]
    public void One_OfType_missing()
    {
        var target = new ResultData("test");
        var result = target.OfType<int>();
        result.Success.Should().BeFalse();
    }

    [Test]
    public void List_OfType()
    {
        var target = new ResultData("test")
            .And(123)
            .And(1.23);

        var r1 = target.OfType<string>();
        r1.Value.Should().Be("test");

        var r2 = target.OfType<int>();
        r2.Value.Should().Be(123);

        var r3 = target.OfType<double>();
        r3.Value.Should().Be(1.23);
    }

    [Test]
    public void List_OfType_Missing()
    {
        var target = new ResultData("test");
        target = target.And(123);

        var result = target.OfType<double>();
        result.Success.Should().BeFalse();
    }

    [Test]
    public void None_OfType()
    {
        var target = new ResultData(null);
        var result = target.OfType<string>();
        result.Success.Should().BeFalse();
    }
}
