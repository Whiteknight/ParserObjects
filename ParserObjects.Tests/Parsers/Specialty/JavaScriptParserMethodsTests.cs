using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.Specialty.JavaScriptStyleParserMethods;

namespace ParserObjects.Tests.Parsers.Specialty
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
    }
}