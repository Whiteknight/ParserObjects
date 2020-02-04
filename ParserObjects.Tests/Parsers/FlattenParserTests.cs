using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class FlattenParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Produce<char, IEnumerable<char>>(() => "abc").Flatten<char, IEnumerable<char>, char>();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Value.Should().Be('a');

            result = parser.Parse(input);
            result.Value.Should().Be('b');

            result = parser.Parse(input);
            result.Value.Should().Be('c');
        }
    }
}
