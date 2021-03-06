﻿using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChooseParserTests
    {
        [Test]
        public void Extension_Basic()
        {
            var parser = Any().Choose(r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aX");
            result.Consumed.Should().Be(2);

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("bY");
            result.Consumed.Should().Be(2);

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("cC");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("cC");
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void Method_Basic()
        {
            var parser = Choose(Any(), r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aX");
            result.Consumed.Should().Be(2);

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("bY");
            result.Consumed.Should().Be(2);

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("cC");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("cC");
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void Extension_InitialFail()
        {
            var parser = Fail<object>().Choose(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Choose(c => (IParser<char, string>)null);
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }
}
