﻿using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class TryParserTests
    {
        [Test]
        public void Parse_Output_Test()
        {
            var target = Try(Any());
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Untyped_Test()
        {
            var target = Try(Empty());
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Output_Fail()
        {
            var target = Try(Fail());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Untyped_Fail()
        {
            var target = Try(End());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Output_Throw()
        {
            var target = Try(Produce<char>(() => throw new System.Exception()));
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Untyped_Throw()
        {
            var inner = (IParser<char>)Produce<char>(() => throw new System.Exception());
            var target = Try(inner);
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Output_Pratt_ControlFlowException()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a control flow exception
            // Try parser should ignore the ControlFlowException, which will bubble up and
            // cause the whole Pratt to fail.
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        ctx.FailAll();
                        return "b";
                    })
                )
            );

            var result = target.Parse("ab");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Output_Pratt_Exception()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a regular exception
            // Try parser should catch this exception, return failure
            // The Pratt will succeed, with "afail"
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        throw new System.Exception();
                    })
                )
            );

            var result = target.Parse("ab");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("afail");
        }
    }
}
