using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FuncParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Function<string>((t, success, fail) => t.Success(null, $"ok:{t.Input.GetNext()}", t.Input.CurrentLocation));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Function<object>((t, success, fail) => fail(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Exception()
        {
            var parser = Function<object>((t, success, fail) => throw new System.Exception("fail"));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Full_Success()
        {
            var parser = Function<object>((t, success, fail) => success($"ok:{t.Input.GetNext()}"));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
        }

        [Test]
        public void Parse_Full_Fail()
        {
            var parser = Function<object>((t, success, fail) => fail(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren()
        {
            var parser = Function<string>((t, success, fail) => t.Success(null, $"ok:{t.Input.GetNext()}", t.Input.CurrentLocation));
            var children = parser.GetChildren().ToList();
            children.Count.Should().Be(0);
        }

    }
}
