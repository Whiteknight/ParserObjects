using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ParserMethodsTests
    {
        [Test]
        public void AnyEnd_Test()
        {
            var parser = Rule(
                Any(),
                End(),
                (a, e) => a
            );
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("ab").Should().BeFalse();
        }
    }
}
