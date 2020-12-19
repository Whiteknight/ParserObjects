using System.Linq;
using FluentAssertions;
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
            result.Consumed.Should().Be(1);
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
            result.Consumed.Should().Be(0);
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
            result.Consumed.Should().Be(1);
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
            result.Consumed.Should().Be(3);
            result.Value.Should().Be(3);
        }

        [Test]
        public void Infix_Addition_TokenDetails()
        {
            var target = Pratt<int>(c => c
                .Add(Integer(), p => p
                    .TypeId(7)
                    .ProduceRight(0, (ctx, v) => v.Value)
                )
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        l.LeftBindingPower.Should().Be(0);
                        l.RightBindingPower.Should().Be(0);
                        l.Value.Should().Be(1);
                        l.TokenTypeId.Should().Be(7);
                        l.IsValid.Should().BeTrue();
                        return l.Value + ctx.Parse();
                    })
                )
            );
            var result = target.Parse("1+2");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
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
            result.Consumed.Should().Be(7);
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
            result.Consumed.Should().Be(9);
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
            result.Consumed.Should().Be(7);
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
            result.Consumed.Should().Be(2);
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
            result.Consumed.Should().Be(2);
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
            result.Consumed.Should().Be(7);
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
            result.Consumed.Should().Be(5);
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
            result.Consumed.Should().Be(6);
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
            result.Consumed.Should().Be(9);
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
            result.Consumed.Should().Be(4);

            result = target.Parse("1234a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("even(1234)");
            result.Consumed.Should().Be(5);
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
            result.Consumed.Should().Be(4);

            result = target.Parse("a1234");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("even(1234)");
            result.Consumed.Should().Be(5);
        }

        [Test]
        public void Empty_ProduceRight()
        {
            // Tests that if a NUD/Prefix rule consumes zero tokens, the parse succeeds.
            var target = Pratt<string>(c => c
                .Add(Produce(() => "a"), p => p
                    .ProduceRight(1, (ctx, op) => "ok")
                )
            );
            var result = target.Parse("123");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            result.Value.Should().Be("ok");
        }

        [Test]
        public void Empty_ProduceRight_FailsOnRecursion()
        {
            // Tests that if a NUD/Prefix rule consumes zero tokens, the user rule is not allowed
            // to recurse. Attempt to recurse into ctx.Parse() or variants will cause the rule to
            // fail.
            var target = Pratt<string>(c => c
                .Add(Produce(() => "a"), p => p
                    .ProduceRight(1, (ctx, op) => "a" + ctx.Parse())
                )
            );
            var result = target.Parse("123");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_ContextRecurseParse()
        {
            var target = Pratt<string>(c => c
                .Add(Identifier())
                .Add(Match('('), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var args = ctx.Parse(SeparatedList(ctx, Match(',')));
                        ctx.Expect(Match(')'));
                        return $"args[0]={args[0]},args[1]={args[1]},method={l}";
                    })
                )
            );
            var result = target.Parse("a(b,c)");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("args[0]=b,args[1]=c,method=a");
        }

        [Test]
        public void Empty_ProduceLeft()
        {
            // Tests that if a LED/Suffix rule consumes zero tokens, we break the loop and only
            // execute the suffix once.
            var target = Pratt<string>(c => c
                .Add(DigitString())
                .Add(Produce(() => "a"), p => p
                    .ProduceLeft(1, (ctx, l, op) => op + l.Value)
                )
            );
            var result = target.Parse("123");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            result.Value.Should().Be("a123");
        }

        [Test]
        public void Parse_ParseParser_Ok()
        {
            var target = Pratt<int>(c => c
                .Add(Integer())
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var prefix = ctx.Parse(Match('D'));
                        return l.Value + 10 * ctx.Parse();
                    })
                )
            );
            var result = target.Parse("1+D2");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(4);
            result.Value.Should().Be(21);
        }

        [Test]
        public void Parse_ParseParser_Fail()
        {
            var target = Pratt<int>(c => c
                .Add(Integer())
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var prefix = ctx.Parse(Match('D'));
                        return l.Value + 10 * ctx.Parse();
                    })
                )
            );
            var result = target.Parse("1+2");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
            result.Value.Should().Be(1);
        }

        [Test]
        public void Parse_TryParseParser_Ok()
        {
            var target = Pratt<int>(c => c
                .Add(Integer())
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var prefix = ctx.TryParse(Match('D'));
                        if (prefix.Success)
                            return l.Value + 10 * ctx.Parse();
                        return l.Value + ctx.Parse();
                    })
                )
            );
            var result = target.Parse("1+D2");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(4);
            result.Value.Should().Be(21);
        }

        [Test]
        public void Parse_TryParseParser_Fail()
        {
            var target = Pratt<int>(c => c
                .Add(Integer())
                .Add(Match('+'), p => p
                    .ProduceLeft(1, (ctx, l, op) =>
                    {
                        var prefix = ctx.TryParse(Match('D'));
                        if (prefix.Success)
                            return l.Value + 10 * ctx.Parse();
                        return l.Value + ctx.Parse();
                    })
                )
            );
            var result = target.Parse("1+2");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            result.Value.Should().Be(3);
        }

        [Test]
        public void GetChildren_Test()
        {
            var a = Any();
            var b = Fail<char>();
            var target = Pratt<char>(p => p
                .Add(a)
                .Reference(b)
            );
            var results = target.GetChildren().ToList();
            results.Count.Should().Be(2);
            results.Should().Contain(a);
            results.Should().Contain(b);
        }
    }
}
