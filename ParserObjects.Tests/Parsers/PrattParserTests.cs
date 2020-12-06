﻿using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class PrattParserTests
    {
        [Test]
        public void SingleNumber()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
            );
            var result = target.Parse("1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void GracefulFail()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
            );
            var input = new StringCharacterSequence("+!@#");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            input.GetRemainder().Should().Be("+!@#");
        }

        [Test]
        public void SingleNumber_Remainder()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
            );
            var input = new StringCharacterSequence("1a");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Infix_Addition()
        {
            var target = Pratt<int>(c => c
                .Add(Integer())
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) => l.Value + ctx.Parse())
                )
            );
            var result = target.Parse("1+2");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(3);
        }

        [Test]
        public void Infix_AdditionSubtractionChain()
        {
            // In most C-like languages and others, +/- have the same precidence and are
            // left associative
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add((Match('+'), Match('-')).First(), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l}{op}{r})";
                    })
                )
            );

            var result = target.Parse("1+2-3+4");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(((1+2)-3)+4)");
        }

        [Test]
        public void Infix_MixedPrecidenceChain()
        {
            // In most C-like languages and others, +/- have the same precidence and are
            // left associative
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add((Match('+'), Match('-')).First(), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l}{op}{r})";
                    })
                )
                .Add((Match('*'), Match('/')).First(), p => p
                    .ProduceLeft(3, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l}{op}{r})";
                    })
                )
            );
            var result = target.Parse("1+2*3+4/5");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("((1+(2*3))+(4/5))");
        }

        [Test]
        public void Infix_EqualsChain()
        {
            // In most C-like languages and others, = is right-associative
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Match('='), p => p
                    .ProduceLeft(2, 1, (ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l}{op}{r})";
                    })
                )
            );
            var result = target.Parse("1=2=3=4");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1=(2=(3=4)))");
        }

        [Test]
        public void Prefix_Negation()
        {
            var target = Pratt<int>(c => c
                .Add(UnsignedInteger())
                .Add(Match('-'), p => p
                    .ProduceRight(1, (ctx, op) => -ctx.Parse())
                )
            );
            var result = target.Parse("-1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(-1);
        }

        [Test]
        public void Postfix_Factorial()
        {
            var target = Pratt<int>(c => c
                .Add(UnsignedInteger())
                .Add(Match('!'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var accumulate = 1;
                        for (int i = 1; i <= l.Value; i++)
                            accumulate = accumulate * i;
                        return accumulate;
                    })
                )
            );
            var result = target.Parse("5!");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(120);
        }

        [Test]
        public void Circumfix_NestedParens()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Match('('), p => p
                    .ProduceRight((ctx, op) =>
                    {
                        var contents = ctx.Parse();
                        ctx.Expect(Match(')'));
                        return contents;
                    })
                )
            );
            var result = target.Parse("(((1)))");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void Circumfix_Brackets()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Match('['), p => p
                    .ProduceRight((ctx, op) =>
                    {
                        var contents = ctx.Parse(0);
                        ctx.Expect(Match(']'));
                        return $"([{contents}])";
                    })
                )
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var right = ctx.Parse();
                        return $"({l}{op}{right})";
                    })
                )
            );
            var result = target.Parse("[1+2]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("([(1+2)])");
        }

        [Test]
        public void Postcircumfix_BracketsIndex()
        {
            var target = Pratt<string>(c => c
               .Add(DigitString())
               .Add(Match('['), p => p
                   .ProduceLeft(1, (ctx, l, op) =>
                   {
                       var contents = ctx.Parse(0);
                       ctx.Expect(Match(']'));
                       return $"({l}[{contents}])";
                   })
               )
               .Add(Match('+'), p => p
                   .ProduceLeft(1, (ctx, l, op) =>
                   {
                       var right = ctx.Parse();
                       return $"({l}{op}{right})";
                   })
               )
            );
            var result = target.Parse("1[2+3]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1[(2+3)])");
        }

        [Test]
        public void MixedAssociation_Test()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Identifier(), p => p
                    .ProduceRight((ctx, value) => value.Value)
                )
                .Add(Match('+'), p => p
                    .ProduceLeft(3, (ctx, left, op) =>
                    {
                        var right = ctx.Parse();
                        return $"({left}+{right})";
                    })
                )
                .Add(Match('='), p => p
                    .ProduceLeft(2, 1, (ctx, left, op) =>
                    {
                        var right = ctx.Parse();
                        return $"({left}={right})";
                    })
                )
            );
            var result = target.Parse("a=b=4+5+6");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(a=(b=((4+5)+6)))");
        }

        [Test]
        public void Postfix_MultiMatch()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Match('a'), p => p
                    .ProduceLeft(1, (ctx, lt, op) =>
                    {
                        var l = lt.Value;
                        if (l.Length % 2 == 0)
                            ctx.FailRule("Expected odd number");
                        return $"odd({l})";
                    })
                    .ProduceLeft(1, (ctx, lt, op) =>
                    {
                        var l = lt.Value;
                        if (l.Length % 2 == 1)
                            ctx.FailRule("Expected even number");
                        return $"even({l})";
                    })
                )
            );
            var result = target.Parse("123a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("odd(123)");

            result = target.Parse("1234a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("even(1234)");
        }

        [Test]
        public void Prefix_MultiMatch()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Match('a'), p => p
                    .ProduceRight(1, (ctx, op) =>
                    {
                        var r = ctx.Parse();
                        if (r.Length % 2 == 0)
                            ctx.FailRule("Expected odd number");
                        return $"odd({r})";
                    })
                    .ProduceRight(1, (ctx, op) =>
                    {
                        var r = ctx.Parse();
                        if (r.Length % 2 == 1)
                            ctx.FailRule("Expected even number");
                        return $"even({r})";
                    })
                )
            );
            var result = target.Parse("a123");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("odd(123)");

            result = target.Parse("a1234");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("even(1234)");
        }
    }
}
