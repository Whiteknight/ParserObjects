using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Identifiers;

internal class SpinalCaseTests
{
    [Test]
    public void SpinalCase_SingleWord()
    {
        var target = SpinalCase();
        var result = target.Parse("test").Value.ToList();
        result.Should().ContainInOrder("test");
    }

    [Test]
    public void SpinalCase_Test()
    {
        var target = SpinalCase();
        var result = target.Parse("this-is-a-test").Value.ToList();
        result.Should().ContainInOrder("this", "is", "a", "test");
    }
}
