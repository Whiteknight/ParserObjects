using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FuncParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Function(t => new SuccessResult<string>($"ok:{t.GetNext()}", t.CurrentLocation));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Function<object>(t => new FailResult<string>());
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Exception()
        {
            var parser = Function<object>(t => throw new System.Exception("fail"));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren()
        {
            var parser = Function(t => new SuccessResult<string>($"ok:{t.GetNext()}", t.CurrentLocation));
            var children = parser.GetChildren().ToList();
            children.Count.Should().Be(0);
        }

    }
}
