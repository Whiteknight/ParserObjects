using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class ContextTests
{
    public class Single
    {
        [Test]
        public void Parse_Test()
        {
            var before = "";
            var after = "";
            var target = Context(
                Produce(() => "OK"),
                _ => before = "OK1",
                _ => after = "OK2"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("OK");
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void Parse_SetupThrows()
        {
            var after = "";
            var target = Context(
                Produce<string>(() => "OK"),
                _ => throw new Exception("test"),
                _ => after = "OK2"
            );
            var result = target.Parse("");
            result.Success.Should().BeFalse();

            after.Should().Be("");

            var exData = result.TryGetData<Exception>();
            exData.Success.Should().BeTrue();
            exData.Value.Message.Should().Be("test");
        }

        [Test]
        public void Parse_InnerThrows()
        {
            var before = "";
            var after = "";
            var target = Context(
                Produce<string>(() => throw new System.Exception("test")),
                _ => before = "OK1",
                _ => after = "OK2"
            );
            Action act = () => target.Parse("");
            act.Should().Throw<Exception>();
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void Parse_CleanupThrows()
        {
            var before = "";
            var after = "";
            var target = Context(
                Produce<string>(() => "OK"),
                _ => before = "OK1",
                _ => throw new Exception("test")
            );
            Action act = () => target.Parse("");
            act.Should().Throw<Exception>();

            before.Should().Be("OK1");
            after.Should().Be("");
        }

        [Test]
        public void Match_Test()
        {
            var before = "";
            var after = "";
            var target = Context(
                Produce(() => "OK"),
                _ => before = "OK1",
                _ => after = "OK2"
            );
            var result = target.Match("");
            result.Should().BeTrue();
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void Match_InnerThrows()
        {
            var before = "";
            var after = "";
            var target = Try(
                Context(
                    Function<string>((state, results) =>
                    {
                        throw new System.Exception();
                    }),
                    _ => before = "OK1",
                    _ => after = "OK2"
                )
            );
            var result = target.Match("");
            result.Should().BeFalse();
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Context(
                Produce(() => "OK"),
                _ => { },
                _ => { }
            ).Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := PRODUCE");
        }
    }

    public class Multi
    {
        [Test]
        public void Parse_Test()
        {
            var before = "";
            var after = "";
            var target = Context(
                ProduceMulti(() => new[] { "OK" }),
                _ => before = "OK1",
                _ => after = "OK2"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("OK");
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void Parse_Throws()
        {
            var before = "";
            var after = "";
            var target = Try(
                Context(
                    ProduceMulti<string>(() => throw new System.Exception()),
                    _ => before = "OK1",
                    _ => after = "OK2"
                )
            );
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            before.Should().Be("OK1");
            after.Should().Be("OK2");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Context(
                ProduceMulti(() => new[] { "OK" }),
                _ => { },
                _ => { }
            ).Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := PRODUCE");
        }
    }
}
