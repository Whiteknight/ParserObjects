using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ExamineParserTests
    {
        [Test]
        public void Method_Test()
        {
            char before = '\0';
            string after = "";
            var parser = Examine(Any(), s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be("ab");
        }

        [Test]
        public void Extension_Test()
        {
            char before = '\0';
            char after = '\0';
            var parser = Any().Examine(s => before = s.Input.Peek(), s => after = s.Input.Peek());
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }

        [Test]
        public void GetChildren_Test()
        {
            char before = '\0';
            string after = "";
            var anyParser = Any();
            var parser = Examine(anyParser, s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var children = parser.GetChildren();
            children.Count().Should().Be(1);
            children.Single().Should().BeSameAs(anyParser);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            char before = '\0';
            string after = "";
            var any = Any();
            var parser = Examine(any, s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var fail = Fail<char>();

            var newParser = parser.ReplaceChild(any, fail);

            var children = newParser.GetChildren();
            children.Count().Should().Be(1);
            children.Single().Should().BeSameAs(fail);
        }
    }
}