using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests
{
    public class CStyleParserMethodsTests
    {
        [Test]
        public void CStyleIntegerLiteral_Tests()
        {
            var parser = Integer();
            var result = parser.Parse(new StringCharacterSequence("1234"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(1234);
        }

        [Test]
        public void CStyleHexadecimalLiteral_Tests()
        {
            var parser = HexadecimalInteger();
            var result = parser.Parse(new StringCharacterSequence("0xAB12"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(0xAB12);
        }

        [Test]
        public void CStyleDoubleLiteral_Tests()
        {
            var parser = Double();
            var result = parser.Parse(new StringCharacterSequence("12.34"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(12.34);
        }

        [Test]
        public void CStyleCommentLiteral_Tests()
        {
            var parser = Comment();
            var result = parser.Parse(new StringCharacterSequence("/* TEST */"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("/* TEST */");
        }

        [Test]
        public void CStyleIdentifier_Tests()
        {
            var parser = Identifier();
            parser.CanMatch("_").Should().BeTrue();
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("a1").Should().BeTrue();
            parser.CanMatch("_1").Should().BeTrue();
            parser.CanMatch("test").Should().BeTrue();
            parser.CanMatch("0").Should().BeFalse();
            parser.CanMatch("-a").Should().BeFalse();
        }

        [Test]
        public void String_Tests()
        {
            var parser = String();
            var result = parser.Parse("\"abcd\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"abcd\"");
        }

        [Test]
        public void String_Escapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\a\\n\\r\\x0A\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"\\a\\n\\r\\x0A\"");
        }

        [Test]
        public void String_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void StrippedString_Tests()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"abcd\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcd");
        }

        [Test]
        public void StrippedString_Escapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"\\a\\n\\r\\x0A\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\a\n\r\x0A");
        }

        [Test]
        public void StrippedString_InvalidEscapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Character_Tests()
        {
            var parser = Character();
            var result = parser.Parse("'a'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'a'");
        }

        [Test]
        public void Character_Escapes()
        {
            var parser = Character();
            var result = parser.Parse("'\\n'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'\\n'");
        }

        [Test]
        public void Character_InvalidEscapes()
        {
            var parser = Character();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void StrippedCharacter_Tests()
        {
            var parser = StrippedCharacter();
            var result = parser.Parse("'a'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void StrippedCharacter_Escapes()
        {
            var parser = StrippedCharacter();
            var result = parser.Parse("'\\n'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('\n');
        }

        [Test]
        public void StrippedCharacter_InvalidEscapes()
        {
            var parser = StrippedCharacter();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }
    }
}
