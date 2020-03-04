﻿using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.Specialty.SqlStyleParserMethods;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class SqlParserMethodsTests
    {
        [Test]
        public void SqlStyleCommentLiteral_Tests()
        {
            var parser = Comment();
            var result = parser.Parse(new StringCharacterSequence("-- TEST\n"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("-- TEST");
        }
    }
}