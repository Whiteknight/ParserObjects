﻿using static ParserObjects.Parsers<char>;

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
                state => before = "OK1",
                state => after = "OK2"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("OK");
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
                    Produce<string>(() => throw new System.Exception()),
                    state => before = "OK1",
                    state => after = "OK2"
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
                Produce(() => "OK"),
                state => { },
                state => { }
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
                state => before = "OK1",
                state => after = "OK2"
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
                    state => before = "OK1",
                    state => after = "OK2"
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
                state => { },
                state => { }
            ).Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := PRODUCE");
        }
    }
}
