using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class GuidsTests
{
    public class N
    {
        [Test]
        public void Parse_Empty()
        {
            var target = GuidN();
            var result = target.Parse("00000000000000000000000000000000");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Empty);
        }

        [Test]
        public void Parse()
        {
            var target = GuidN();
            var result = target.Parse("123456781234123412341234567890AB");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Parse("12345678-1234-1234-1234-1234567890AB"));
        }

        [Test]
        public void Name()
        {
            var target = GuidN();
            target.Name.Should().Be("Guid(N)");
        }
    }

    public class D
    {
        [Test]
        public void Parse_Empty()
        {
            var target = GuidD();
            var result = target.Parse("00000000-0000-0000-0000-000000000000");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Empty);
        }

        [Test]
        public void Parse()
        {
            var target = GuidD();
            var result = target.Parse("12345678-1234-1234-1234-1234567890AB");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Parse("12345678-1234-1234-1234-1234567890AB"));
        }

        [Test]
        public void Name()
        {
            var target = GuidD();
            target.Name.Should().Be("Guid(D)");
        }
    }

    public class B
    {
        [Test]
        public void Parse_Empty()
        {
            var target = GuidB();
            var result = target.Parse("{00000000-0000-0000-0000-000000000000}");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Empty);
        }

        [Test]
        public void Parse()
        {
            var target = GuidB();
            var result = target.Parse("{12345678-1234-1234-1234-1234567890AB}");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Parse("12345678-1234-1234-1234-1234567890AB"));
        }

        [Test]
        public void Name()
        {
            var target = GuidB();
            target.Name.Should().Be("Guid(B)");
        }
    }

    public class P
    {
        [Test]
        public void Parse_Empty()
        {
            var target = GuidP();
            var result = target.Parse("(00000000-0000-0000-0000-000000000000)");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Empty);
        }

        [Test]
        public void Parse()
        {
            var target = GuidP();
            var result = target.Parse("(12345678-1234-1234-1234-1234567890AB)");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(Guid.Parse("12345678-1234-1234-1234-1234567890AB"));
        }

        [Test]
        public void Name()
        {
            var target = GuidP();
            target.Name.Should().Be("Guid(P)");
        }
    }
}
