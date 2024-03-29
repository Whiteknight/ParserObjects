﻿using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

internal class DoubleTests
{
    [Test]
    public void CStyleDoubleLiteral_Tests()
    {
        var parser = Double();
        var result = parser.Parse("12.34");
        result.Success.Should().BeTrue();
        result.Value.Should().Be(12.34);
    }
}
