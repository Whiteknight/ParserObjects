using System.Collections.Generic;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class WithDataContextTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Rule(
            SetData("test", "value1"),
            Rule(
                SetData("test", "value2"),
                GetData<string>("test"),
                (a, b) => b
            ).WithDataContext(),
            GetData<string>("test"),
            (a, b, c) => $"{a}-{b}-{c}"
        );
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("value1-value2-value1");
    }

    [Test]
    public void Parse_SingleValue()
    {
        var target = Rule(
            SetData("test", "value1"),
            GetData<string>("test").WithDataContext("test", "value2"),
            GetData<string>("test"),
            (a, b, c) => $"{a}-{b}-{c}"
        );
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("value1-value2-value1");
    }

    [Test]
    public void Parse_DictValue()
    {
        var target = Rule(
            SetData("test", "value1"),
            GetData<string>("test").WithDataContext(new Dictionary<string, object>
            {
                { "test", "value2" }
            }),
            GetData<string>("test"),
            (a, b, c) => $"{a}-{b}-{c}"
        );
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("value1-value2-value1");
    }
}
