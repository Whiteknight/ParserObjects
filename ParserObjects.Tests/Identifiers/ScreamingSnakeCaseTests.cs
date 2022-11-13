using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Identifiers;

internal class ScreamingSnakeCaseTests
{
    [Test]
    public void ScreamingSnakeCase_SingleWord()
    {
        var target = ScreamingSnakeCase();
        var result = target.Parse("TEST").Value.ToList();
        result.Should().ContainInOrder("TEST");
    }

    [Test]
    public void ScreamingSnakeCase_Test()
    {
        var target = ScreamingSnakeCase();
        var result = target.Parse("THIS_IS_A_TEST").Value.ToList();
        result.Should().ContainInOrder("THIS", "IS", "A", "TEST");
    }
}
