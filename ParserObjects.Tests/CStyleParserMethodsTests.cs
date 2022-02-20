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
        [TestCase("\"\\\"\"")]
        [TestCase("\"\\a\"")]
        [TestCase("\"\\b\"")]
        [TestCase("\"\\n\"")]
        [TestCase("\"\\r\"")]
        [TestCase("\"\\v\"")]
        [TestCase("\"\\60\"")]
        [TestCase("\"\\101\"")]
        [TestCase("\"\\x41\"")]
        [TestCase("\"\\x041\"")]
        [TestCase("\"\\x0041\"")]
        [TestCase("\"\\u0026\"")]
        [TestCase("\"\\U00000027\"")]
        public void String_Escapes(string value)
        {
            var parser = String();
            var result = parser.Parse(value);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        [Test]
        public void String_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("\"")]
        [TestCase("test\"")]
        [TestCase("\"\\\"")]
        [TestCase("\"\\x\"")]
        [TestCase("\"\\u\"")]
        [TestCase("\"\\U\"")]
        public void String_Fail(string attempt)
        {
            var parser = String();
            var result = parser.Parse(attempt);
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
        [TestCase("\"\\\"\"", "\"")]
        [TestCase("\"\\a\"", "\a")]
        [TestCase("\"\\b\"", "\b")]
        [TestCase("\"\\n\"", "\n")]
        [TestCase("\"\\r\"", "\r")]
        [TestCase("\"\\v\"", "\v")]
        [TestCase("\"\\101\"", "A")]
        [TestCase("\"\\46\"", "&")]
        [TestCase("\"\\x61\"", "a")]
        [TestCase("\"\\x061\"", "a")]
        [TestCase("\"\\x0061\"", "a")]
        [TestCase("\"\\u0061\"", "a")]
        [TestCase("\"\\U00000061\"", "a")]
        public void StrippedString_Escapes(string input, string expected)
        {
            var parser = StrippedString();
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(expected);
        }

        [Test]
        public void StrippedString_InvalidEscapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("\"")]
        [TestCase("test\"")]
        [TestCase("\"\\\"")]
        [TestCase("\"\\x\"")]
        [TestCase("\"\\u\"")]
        [TestCase("\"\\U\"")]
        public void StrippedString_Fail(string attempt)
        {
            var parser = StrippedString();
            var result = parser.Parse(attempt);
            result.Success.Should().BeFalse();
        }

        [Test]
        [TestCase("'a'", "'a'")]
        [TestCase("'\\n'", "'\\n'")]
        [TestCase("'\\101'", "'\\101'")]
        [TestCase("'\\x60'", "'\\x60'")]
        [TestCase("'\\x060'", "'\\x060'")]
        [TestCase("'\\x0060'", "'\\x0060'")]
        [TestCase("'\\u0060'", "'\\u0060'")]
        [TestCase("'\\U00000060'", "'\\U00000060'")]
        public void Character_Tests(string input, string expected)
        {
            var parser = Character();
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(expected);
        }

        [Test]
        public void Character_InvalidEscapes()
        {
            var parser = Character();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("'")]
        [TestCase("test'")]
        [TestCase("'\\'")]
        [TestCase("'\\x'")]
        [TestCase("'\\u'")]
        [TestCase("'\\U'")]
        public void Character_Fail(string input)
        {
            var parser = Character();
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        [TestCase("'a'", 'a')]
        [TestCase("'\\n'", '\n')]
        [TestCase("'\\101'", 'A')]
        [TestCase("'\\x61'", 'a')]
        [TestCase("'\\x061'", 'a')]
        [TestCase("'\\x0061'", 'a')]
        [TestCase("'\\u0061'", 'a')]
        [TestCase("'\\U00000061'", 'a')]
        public void StrippedCharacter_Tests(string input, char expected)
        {
            var parser = StrippedCharacter();
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(expected);
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

        [Test]
        [TestCase("")]
        [TestCase("'")]
        [TestCase("test'")]
        [TestCase("'\\'")]
        [TestCase("'\\x'")]
        [TestCase("'\\u'")]
        [TestCase("'\\U'")]
        public void StrippedCharacter_Fail(string input)
        {
            var parser = StrippedCharacter();
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }
    }
}
