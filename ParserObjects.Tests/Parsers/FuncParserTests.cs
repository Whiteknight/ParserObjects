using System;
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
            var parser = Function<object>((t, success, fail) => t.Success(null, $"ok:{t.Input.GetNext()}", 0, t.Input.CurrentLocation));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Function<object>((t, success, fail) => fail(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Exception()
        {
            var parser = Function<object>((t, success, fail) => throw new System.Exception("fail"));
            Action act = () => parser.Parse("X");
            act.Should().Throw<Exception>();
        }

        [Test]
        public void Parse_Full_Success()
        {
            var parser = Function<object>((t, success, fail) => success($"ok:{t.Input.GetNext()}"));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Full_Fail()
        {
            var parser = Function<object>((t, success, fail) => fail(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren()
        {
            var parser = Function<object>((t, success, fail) => t.Success(null, $"ok:{t.Input.GetNext()}", 0, t.Input.CurrentLocation));
            var children = parser.GetChildren().ToList();
            children.Count.Should().Be(0);
        }
    }
}
