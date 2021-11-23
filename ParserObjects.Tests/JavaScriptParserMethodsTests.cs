using ParserObjects.Sequences;
using static ParserObjects.JavaScriptStyleParserMethods;

namespace ParserObjects.Tests
{
    public class JavaScriptParserMethodsTests
    {
        [Test]
        public void JavaScriptStyleNumberLiteral_Tests()
        {
            var parser = Number();
            var result = parser.Parse(new StringCharacterSequence("-1.23e+4"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(-12300.0);
        }

        [Test]
        public void String_SingleQuote_Tests()
        {
            var parser = String();
            var result = parser.Parse("'abcd'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'abcd'");
        }

        [Test]
        public void String_SingleQuote_Escapes()
        {
            var parser = String();
            var result = parser.Parse("'\\f\\n\\r\\x0A'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'\\f\\n\\r\\x0A'");
        }

        [Test]
        public void String_SingleQuote_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void String_DoubleQuote_Tests()
        {
            var parser = String();
            var result = parser.Parse("\"abcd\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"abcd\"");
        }

        [Test]
        public void String_DoubleQuote_Escapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\f\\n\\r\\x0A\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"\\f\\n\\r\\x0A\"");
        }

        [Test]
        public void String_DoubleQuote_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void StrippedString_SingleQuote_Tests()
        {
            var parser = StrippedString();
            var result = parser.Parse("'abcd'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcd");
        }

        [Test]
        public void StrippedString_SingleQuote_Escapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("'\\f\\n\\r\\x0A'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\f\n\r\x0A");
        }

        [Test]
        public void StrippedString_SingleQuote_InvalidEscapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void StrippedString_DoubleQuote_Tests()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"abcd\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcd");
        }

        [Test]
        public void StrippedString_DoubleQuote_Escapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"\\f\\n\\r\\x0A\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\f\n\r\x0A");
        }

        [Test]
        public void StrippedString_DoubleQuote_InvalidEscapes()
        {
            var parser = StrippedString();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }
    }
}