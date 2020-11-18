using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ParserMethodsTests
    {
        [Test]
        public void AnyEnd_Test()
        {
            var parser = Any().FollowedBy(End());
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("ab").Should().BeFalse();
        }
    }
}
