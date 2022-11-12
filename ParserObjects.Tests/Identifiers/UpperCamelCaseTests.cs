using System.Linq;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Identifiers;

internal class UpperCamelCaseTests
{
    [Test]
    public void UpperCamelCase_AllLower()
    {
        var target = UpperCamelCase();
        var result = target.Parse("test");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void UpperCamelCase_Test()
    {
        var target = UpperCamelCase();
        var result = target.Parse("ThisIsATest").Value.ToList();
        result.Should().ContainInOrder("This", "Is", "A", "Test");
    }

    [Test]
    public void UpperCamelCase_StartsLower()
    {
        var target = UpperCamelCase();
        var result = target.Parse("thisIsATest");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void UpperCamelCase_ContainsAcronym()
    {
        var target = UpperCamelCase();
        var result = target.Parse("ThisIsABCTest").Value.ToList();
        result.Should().ContainInOrder("This", "Is", "ABC", "Test");
    }

    [Test]
    public void UpperCamelCase_StartsAcronym()
    {
        var target = UpperCamelCase();
        var result = target.Parse("ABCThisIsTest").Value.ToList();
        result.Should().ContainInOrder("ABC", "This", "Is", "Test");
    }

    [Test]
    public void UpperCamelCase_Fail_StartsNumber()
    {
        var target = UpperCamelCase();
        var result = target.Parse("123ThisIsTest");
        result.Success.Should().BeFalse();
    }
}
