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
            var parser = ProgrammingParserMethods.CPlusPlusStyleComment();
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

        [Test]
        public void JavaScriptStyleNumberLiteral_Tests()
        {
            var parser = ProgrammingParserMethods.JavaScriptStyleNumberLiteral();
            var result = parser.Parse(new StringCharacterSequence("-1.23e+4"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(-12300.0);
        }

        [Test]
        public void CStyleIdentifier_Tests()
        {
            var parser = ProgrammingParserMethods.CStyleIdentifier();
            parser.CanMatch("_").Should().BeTrue();
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("a1").Should().BeTrue();
            parser.CanMatch("_1").Should().BeTrue();
            parser.CanMatch("test").Should().BeTrue();
            parser.CanMatch("0").Should().BeFalse();
            parser.CanMatch("-a").Should().BeFalse();
        }

        [Test]
        public void DoubleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = ProgrammingParserMethods.DoubleQuotedStringWithEscapedQuotes();
            var result = parser.Parse("\"TEST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"TEST\"");

            result = parser.Parse("\"TE\\\"ST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"TE\\\"ST\"");
        }

        [Test]
        public void SingleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = ProgrammingParserMethods.SingleQuotedStringWithEscapedQuotes();
            var result = parser.Parse("'TEST'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'TEST'");
        }

        [Test]
        public void StrippedDoubleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = ProgrammingParserMethods.StrippedDoubleQuotedStringWithEscapedQuotes();
            var result = parser.Parse("\"TEST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");

            result = parser.Parse("\"TE\\\"ST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TE\"ST");
        }

        [Test]
        public void StrippedSingleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = ProgrammingParserMethods.StrippedSingleQuotedStringWithEscapedQuotes();
            var result = parser.Parse("'TEST'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");
        }
    }
}
