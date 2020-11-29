using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class DeferredParserTests
    {
        [Test]
        public void Parse_Any()
        {
            var target = Deferred(() => Any());
            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('b');
            target.Parse(input).Value.Should().Be('c');
            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var target = Deferred(() => anyParser);
            var result = target.GetChildren().ToList();
            result.Count().Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void Parse_null()
        {
            var target = Deferred(() => (IParser<char, char>)null);
            Action act = () => target.Parse("abc");
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
