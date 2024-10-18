using static ParserObjects.Parsers.Sql;

namespace ParserObjects.Tests.Parsers.Sql;

internal class IdentifierTests
{
    [TestCase("abc", "abc")]
    [TestCase("_@#$", "_@#$")]
    [TestCase("[abc]", "abc")]
    [TestCase("[a b c]", "a b c")]
    [TestCase("[]", "")]
    [TestCase("'abc'", "abc")]
    [TestCase("'a b c'", "a b c")]
    [TestCase("''", "")]
    [TestCase("\"abc\"", "abc")]
    [TestCase("\"a b c\"", "a b c")]
    [TestCase("\"a\"\"b\"\"c\"", "a\"b\"c")]
    [TestCase("\"\"", "")]
    public void Test(string input, string expected)
    {
        var target = Identifier();
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("$abc")]
    [TestCase("")]
    public void Fail(string input)
    {
        var target = Identifier();
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }
}
