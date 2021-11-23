using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ContinueWithParserTests
    {
        [Test]
        public void ContinueWith_Single()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .ContinueWith(left =>
                    Rule(
                        left,
                        Produce(() => 'X'),
                        (l, x) => $"{l}{x}"
                    )
                );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("AX");
            result.Results[1].Value.Should().Be("BX");
        }

        [Test]
        public void ContinueWith_Multi()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .ContinueWith(left =>
                    Each(
                        Rule(left, Any(), (l, x) => $"{l}{x}"),
                        Rule(left, Produce(() => 'Y'), (l, x) => $"{l}{x}")
                    )
                );
            var result = target.Parse("X");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("AX");
            result.Results[1].Value.Should().Be("AY");
            result.Results[2].Value.Should().Be("BX");
            result.Results[3].Value.Should().Be("BY");
        }

        [Test]
        public void ContinueWithEach_Multi()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .ContinueWithEach(left => new[]
                {
                    Rule(left, Any(), (l, x) => $"{l}{x}"),
                    Rule(left, Produce(() => 'Y'), (l, x) => $"{l}{x}")
                });
            var result = target.Parse("X");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("AX");
            result.Results[1].Value.Should().Be("AY");
            result.Results[2].Value.Should().Be("BX");
            result.Results[3].Value.Should().Be("BY");
        }

        [Test]
        public void Transform_Test()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .Transform(l => $"{l}X");
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("AX");
            result.Results[1].Value.Should().Be("BX");
        }

        [Test]
        public void ToBnf_Single()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .ContinueWith(left =>
                    Rule(
                        left,
                        Produce(() => 'X'),
                        (l, x) => $"{l}{x}"
                    )
                );
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := PRODUCE CONTINUEWITH (<(TARGET)> PRODUCE)");
        }

        [Test]
        public void ToBnf_Multi()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .ContinueWith(left =>
                    Each(
                        Rule(left, Any(), (l, x) => $"{l}{x}"),
                        Rule(left, Produce(() => 'Y'), (l, x) => $"{l}{x}")
                    )
                );
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := PRODUCE CONTINUEWITH EACH((<(TARGET)> .) | (<(TARGET)> PRODUCE))");
        }
    }
}
