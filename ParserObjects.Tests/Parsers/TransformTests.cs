﻿using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class TransformTests
{
    public class SingleFunction
    {
        [Test]
        public void Parse_Test()
        {
            var any = Any();
            var parser = Transform(
                any,
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Value.Should().Be(1);
            result.Parser.Should().BeSameAs(parser);
        }

        [Test]
        public void Parse_Failure()
        {
            var fail = Fail<char>();
            var parser = Transform(
                fail,
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Success.Should().BeFalse();
            result.Parser.Should().BeSameAs(fail);
        }

        [Test]
        public void Parse_Consumed()
        {
            var parser = Transform(
                Any(),
                c => int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Value.Should().Be(1);
            result.Consumed.Should().Be(1);

            result = parser.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();
            var parser = Transform(
                failParser,
                c => int.Parse(c.ToString())
            );
            var results = parser.GetChildren().ToList();
            results.Count.Should().Be(1);
            results[0].Should().BeSameAs(failParser);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Transform(Any(), x => x).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .");
        }
    }

    public class SingleFunctionData
    {
        [Test]
        public void Parse_Test()
        {
            var any = Any();
            var parser = Transform(
                any,
                25,
                (d, c) => d + int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Value.Should().Be(26);
            result.Parser.Should().BeSameAs(parser);
        }

        [Test]
        public void Parse_Failure()
        {
            var fail = Fail<char>();
            var parser = Transform(
                fail,
                25,
                (d, c) => d + int.Parse(c.ToString())
            );
            var result = parser.Parse("1");
            result.Success.Should().BeFalse();
            result.Parser.Should().BeSameAs(fail);
        }
    }

    public class MultiFunction
    {
        [Test]
        public void Parse_Multi()
        {
            var parser = Transform(
                ProduceMulti(() => new[] { "1" }),
                c => int.Parse(c.ToString())
            );
            parser.Parse("").Results[0].Value.Should().Be(1);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Transform(
                ProduceMulti(() => new[] { "1" }),
                c => int.Parse(c)
            ).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := PRODUCE");
        }
    }

    public class SingleExtension
    {
        [Test]
        public void Parse_Extension()
        {
            var parser = Any()
                .Transform(c => int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Any().Transform(x => x).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .");
        }
    }

    public class SingleExtensionData
    {
        [Test]
        public void Parse_Extension()
        {
            var parser = Any()
                .Transform(25, (d, c) => d + int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(26);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Any().Transform(x => x).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .");
        }
    }

    public class MultiExtension
    {
        [Test]
        public void Parse_Extension_Multi()
        {
            var parser = ProduceMulti(() => new[] { "1" })
                .Transform(c => int.Parse(c.ToString()));
            parser.Parse("").Results[0].Value.Should().Be(1);
        }

        [Test]
        public void Parse_ExtensionOnMulti_Test()
        {
            var target = ProduceMulti(() => new[] { 'A', 'B' })
                .Transform(l => $"{l}X");
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("AX");
            result.Results[1].Value.Should().Be("BX");
        }
    }
}
