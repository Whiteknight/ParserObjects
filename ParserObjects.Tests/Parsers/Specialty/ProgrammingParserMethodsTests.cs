using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers.Specialty;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class ProgrammingParserMethodsTests
    {
        [Test]
        public void CStyleIntegerLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.CStyleIntegerLiteral();
            var result = parser.Parse(new StringCharacterSequence("1234"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(1234);
        }

        [Test]
        public void CStyleHexadecimalLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.CStyleHexadecimalLiteral();
            var result = parser.Parse(new StringCharacterSequence("0xAB12"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(0xAB12);
        }

        [Test]
        public void CStyleDoubleLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.CStyleDoubleLiteral();
            var result = parser.Parse(new StringCharacterSequence("12.34"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(12.34);
        }

        [Test]
        public void CStyleCommentLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.CStyleComment();
            var result = parser.Parse(new StringCharacterSequence("/* TEST */"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("/* TEST */");
        }

        [Test]
        public void CPlusPlusStyleCommentLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.CPlusPlussStyleComment();
            var result = parser.Parse(new StringCharacterSequence("// TEST\n"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("// TEST");
        }

        [Test]
        public void SqlStyleCommentLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.SqlStyleComment();
            var result = parser.Parse(new StringCharacterSequence("-- TEST\n"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("-- TEST");
        }
    }
}
