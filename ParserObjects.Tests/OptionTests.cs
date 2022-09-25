namespace ParserObjects.Tests;

public class OptionTests
{
    [TestCase("test")]
    public void Success_FirstFunctorLaw(string value)
    {
        static string Id(string x) => x;
        var opt = new SuccessOption<string>(value);
        opt.Select(Id).Success.Should().BeTrue();
        opt.Select(Id).Value.Should().Be(value);
    }

    [TestCase("test")]
    public void Success_SecondFunctorLaw(string value)
    {
        Func<string, int> g = s => s.Length;
        Func<int, bool> f = i => i % 2 == 0;
        var m = new SuccessOption<string>(value);

        m.Select(g).Select(f).Should().Be(m.Select(s => f(g(s))));
    }

    [Test]
    public void Failure_FirstFunctorLaw()
    {
        static string Id(string x) => x;
        var opt = FailureOption<string>.Instance;
        opt.Select(Id).Success.Should().BeFalse();
    }

    [Test]
    public void Failure_SecondFunctorLaw()
    {
        Func<string, int> g = s => s.Length;
        Func<int, bool> f = i => i % 2 == 0;
        var opt = FailureOption<string>.Instance;

        opt.Select(g).Select(f).Should().Be(opt.Select(s => f(g(s))));
    }

    [TestCase("test", 4)]
    public void Success_SelectMany(string value, int expectedLength)
    {
        var opt = new SuccessOption<string>(value);
        var result = opt.SelectMany(s => new SuccessOption<int>(s.Length));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expectedLength);
    }

    [Test]
    public void Failure_SelectMany()
    {
        var opt = FailureOption<string>.Instance;
        var result = opt.SelectMany(s => new SuccessOption<int>(s.Length));
        result.Success.Should().BeFalse();
    }
}
