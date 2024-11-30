using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests;

public class ResultTests
{
    [Test]
    public void ErrorMessage_Error()
    {
        var target = Result.Fail(Any(), "Fail");
        target.ErrorMessage.Should().Be("Fail");
    }

    [Test]
    public void ErrorMessage_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.ErrorMessage.Should().BeEmpty();
    }

    [Test]
    public void Value_Error()
    {
        var target = Result.Fail(Any(), "Fail");
        Action act = () => { var x = target.Value; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Value_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.Value.Should().Be("Message");
    }

    [Test]
    public void GetValueOrDefault_Error()
    {
        var target = Result.Fail<string>(Any(), "Fail");
        target.GetValueOrDefault("OK").Should().Be("OK");
    }

    [Test]
    public void GetValueOrDefault_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.GetValueOrDefault("OK").Should().Be("Message");
    }

    [Test]
    public void GetValueOrDefault_Func_Error()
    {
        var target = Result.Fail<string>(Any(), "Fail");
        target.GetValueOrDefault(() => "OK").Should().Be("OK");
    }

    [Test]
    public void GetValueOrDefault_Func_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.GetValueOrDefault(() => "OK").Should().Be("Message");
    }

    [Test]
    public void Select_Error()
    {
        var target = Result.Fail<string>(Any(), "Fail");
        target.Select(m => "[" + m + "]").Success.Should().BeFalse();
    }

    [Test]
    public void Select_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.Select(m => "[" + m + "]").Value.Should().Be("[Message]");
    }

    [Test]
    public void ToString_Error()
    {
        var target = Result.Fail<string>(Any(), "Fail");
        target.ToString().Should().Contain("FAIL");
    }

    [Test]
    public void ToString_Success()
    {
        var target = Result.Ok(Any(), "Message", 0);
        target.ToString().Should().Contain("Ok");
    }
}
