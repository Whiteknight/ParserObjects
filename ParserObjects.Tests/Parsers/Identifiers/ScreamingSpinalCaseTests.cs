using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers.Identifiers;

internal class ScreamingSpinalCaseTests
{
    [Test]
    public void ScreamingSpinalCase_SingleWord()
    {
        var target = ScreamingSpinalCase();
        var result = target.Parse("TEST").Value.ToList();
        result.Should().ContainInOrder("TEST");
    }

    [Test]
    public void ScreamingSpinalCase_Test()
    {
        var target = ScreamingSpinalCase();
        var result = target.Parse("THIS-IS-A-TEST").Value.ToList();
        result.Should().ContainInOrder("THIS", "IS", "A", "TEST");
    }
}
