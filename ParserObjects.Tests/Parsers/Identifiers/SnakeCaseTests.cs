﻿using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers.Identifiers;

internal class SnakeCaseTests
{
    [Test]
    public void SnakeCase_SingleWord()
    {
        var target = SnakeCase();
        var result = target.Parse("test").Value.ToList();
        result.Should().ContainInOrder("test");
    }

    [Test]
    public void SnakeCase_Test()
    {
        var target = SnakeCase();
        var result = target.Parse("this_is_a_test").Value.ToList();
        result.Should().ContainInOrder("this", "is", "a", "test");
    }
}
