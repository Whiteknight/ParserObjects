using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Identifiers;

internal class LowerCamelCaseTests
{
    [Test]
    public void LowerCamelCase_AllLower()
    {
        var target = LowerCamelCase();
        var result = target.Parse("test").Value.ToList();
        result.Should().ContainInOrder("test");
    }

    [Test]
    public void LowerCamelCase_Fail()
    {
        var target = LowerCamelCase();
        var result = target.Parse("ThisIsATest");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void LowerCamelCase_StartsLower()
    {
        var target = LowerCamelCase();
        var result = target.Parse("thisIsATest").Value.ToList();
        result.Should().ContainInOrder("this", "Is", "A", "Test");
    }

    [Test]
    public void LowerCamelCase_ContainsAcronym()
    {
        var target = LowerCamelCase();
        var result = target.Parse("thisIsABCTest").Value.ToList();
        result.Should().ContainInOrder("this", "Is", "ABC", "Test");
    }

    [Test]
    public void LowerCamelCase_StartsAcronym()
    {
        var target = LowerCamelCase();
        var result = target.Parse("ABCThisIsTest");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void LowerCamelCase_Fail_StartsNumber()
    {
        var target = LowerCamelCase();
        var result = target.Parse("123ThisIsTest");
        result.Success.Should().BeFalse();
    }
}
