using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ExamineParserTests
    {
        [Test]
        public void Method_Test()
        {
            char before = '\0';
            string after = "";
            var parser = Examine(Any(), i => before = i.Peek(), (i, r) => after = $"{r.Value}{i.Peek()}");
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
            var parser = Any().Examine(i => before = i.Peek(), (i, r) => after = i.Peek());
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }
    }
}