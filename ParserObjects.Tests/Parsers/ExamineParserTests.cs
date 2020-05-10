using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class ExamineParserTests
    {
        [Test]
        public void Method_Test()
        {
            char before = '\0';
            char after = '\0';
            var parser = ParserMethods.Examine(ParserMethods.Any<char>(), i => before = i.Peek(), (i, r) => after = i.Peek());
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }

        [Test]
        public void Extension_Test()
        {
            char before = '\0';
            char after = '\0';
            var parser = ParserMethods.Any<char>().Examine(i => before = i.Peek(), (i, r) => after = i.Peek());
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }
    }
}