namespace ParserObjects.Tests;

public class OptionTests
{
    [TestCase("test")]
    public void Success_FirstFunctorLaw(string value)
    {
        static string Id(string x) => x;
        var opt = new Option<string>(true, value);
        opt.Select(Id).Success.Should().BeTrue();
        opt.Select(Id).Value.Should().Be(value);
    }

    [TestCase("test")]
    public void Success_SecondFunctorLaw(string value)
    {
        Func<string, int> g = s => s.Length;
        Func<int, bool> f = i => i % 2 == 0;
        var m = new Option<string>(true, value);

        m.Select(g).Select(f).Should().Be(m.Select(s => f(g(s))));
    }

    [Test]
    public void Failure_FirstFunctorLaw()
    {
        static string Id(string x) => x;
        var opt = new Option<string>(false, default);
        opt.Select(Id).Success.Should().BeFalse();
    }

    [Test]
    public void Failure_SecondFunctorLaw()
    {
        Func<string, int> g = s => s.Length;
        Func<int, bool> f = i => i % 2 == 0;
        var opt = new Option<string>(false, default);

        opt.Select(g).Select(f).Should().Be(opt.Select(s => f(g(s))));
    }

    [TestCase("test", 4)]
    public void Success_SelectMany(string value, int expectedLength)
    {
        var opt = new Option<string>(true, value);
        var result = opt.SelectMany(s => new Option<int>(true, s.Length));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expectedLength);
    }

    [Test]
    public void Failure_SelectMany()
    {
        var opt = new Option<string>(false, default);
        var result = opt.SelectMany(s => new Option<int>(true, s.Length));
        result.Success.Should().BeFalse();
    }

    [TestCase(true, "test", true)]
    [TestCase(false, "test", false)]
    [TestCase(true, "XXXX", false)]
    public void Is_Test(bool success, string result, bool expected)
    {
        var opt = new Option<string>(success, "test");
        opt.Is(result).Should().Be(expected);
    }

    [Test]
    public void GetHashCode_Test()
    {
        var opt = new Option<string>(true, "test");
        opt.GetHashCode().Should().Be("test".GetHashCode());

        var fail = new Option<string>(false, "test");
        fail.GetHashCode().Should().Be(0);
    }
}
