using System.Linq;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Identifiers;

internal class CamelCaseTests
{
    [Test]
    public void CamelCase_AllLower()
    {
        var target = CamelCase();
        var result = target.Parse("test").Value.ToList();
        result.Should().ContainInOrder("test");
    }

    [Test]
    public void CamelCase_Test()
    {
        var target = CamelCase();
        var result = target.Parse("ThisIsATest").Value.ToList();
        result.Should().ContainInOrder("This", "Is", "A", "Test");
    }

    [Test]
    public void CamelCase_StartsLower()
    {
        var target = CamelCase();
        var result = target.Parse("thisIsATest").Value.ToList();
        result.Should().ContainInOrder("this", "Is", "A", "Test");
    }

    [Test]
    public void CamelCase_ContainsAcronym()
    {
        var target = CamelCase();
        var result = target.Parse("ThisIsABCTest").Value.ToList();
        result.Should().ContainInOrder("This", "Is", "ABC", "Test");
    }

    [Test]
    public void CamelCase_StartsAcronym()
    {
        var target = CamelCase();
        var result = target.Parse("ABCThisIsTest").Value.ToList();
        result.Should().ContainInOrder("ABC", "This", "Is", "Test");
    }

    [Test]
    public void CamelCase_Fail_StartsNumber()
    {
        var target = CamelCase();
        var result = target.Parse("123ThisIsTest");
        result.Success.Should().BeFalse();
    }
}
