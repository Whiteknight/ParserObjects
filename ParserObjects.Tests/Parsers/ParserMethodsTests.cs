using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class ParserMethodsTests
    {
        [Test]
        public void AnyEnd_Test()
        {
            var parser = Rule(
                Any<char>(),
                End<char>(),
                (a, e) => a
            );
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("ab").Should().BeFalse();
        }
    }
}
