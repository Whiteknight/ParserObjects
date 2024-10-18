using static ParserObjects.Parsers.Sql;

namespace ParserObjects.Tests.Parsers.Sql;

public class QualifiedIdentifierTests
{
    [Test]
    public void Test()
    {
        var target = QualifiedIdentifier();
        var result = target.Parse("test.[abc].'123'.\"this works\"");
        result.Success.Should().BeTrue();
        result.Value.Should().ContainInOrder("test", "abc", "123", "this works");
    }

    [TestCase("")]
    public void Fail(string input)
    {
        var target = QualifiedIdentifier();
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }
}
