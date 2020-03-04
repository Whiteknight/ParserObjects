using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.Specialty.CPlusPlusStyleParserMethods;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class CPlusPlusParserMethodsTests
    {
        [Test]
        public void CPlusPlusStyleCommentLiteral_Tests()
        {
            var parser = Comment();
            var result = parser.Parse(new StringCharacterSequence("// TEST\n"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("// TEST");
        }
    }
}