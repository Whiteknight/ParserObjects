using NUnit.Framework;
using static ParserObjects.CStyleParserMethods;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.ParserMethods;
using FluentAssertions;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class PrattParserTests
    {
        [Test]
        public void SingleNumber()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
            );
            var result = target.Parse("1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void GracefulFail()
        {
            var target = Pratt<string>(c => c
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
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
                .Add(Integer(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('+'), p => p
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) => l.Value + ctx.Parse())
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add((Match('+'), Match('-')).First(), p => p
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add((Match('+'), Match('-')).First(), p => p
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
                    {
                        var r = ctx.Parse();
                        return $"({l}{op}{r})";
                    })
                )
                .Add((Match('*'), Match('/')).First(), p => p
                    .LeftBindingPower(3)
                    .ProduceLeft((ctx, l, op) =>
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('='), p => p
                    .LeftBindingPower(2)
                    .RightBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
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
                .Add(UnsignedInteger(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('-'), p => p
                    .LeftBindingPower(1)
                    .ProduceRight((ctx, op) => -ctx.Parse())
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
                .Add(UnsignedInteger(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('!'), p => p
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('('), p => p
                    .LeftBindingPower(0)
                    .ProduceRight((ctx, op) =>
                    {
                        var contents = ctx.Parse(0);
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
                .Add(DigitString(), p => p
                    .ProduceRight((ctx, v) => v.Value)
                )
                .Add(Match('['), p => p
                    .LeftBindingPower(0)
                    .ProduceRight((ctx, op) =>
                    {
                        var contents = ctx.Parse(0);
                        ctx.Expect(Match(']'));
                        return $"([{contents}])";
                    })
                )
                .Add(Match('+'), p => p
                    .LeftBindingPower(1)
                    .ProduceLeft((ctx, l, op) =>
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
               .Add(DigitString(), p => p
                   .ProduceRight((ctx, v) => v.Value)
               )
               .Add(Match('['), p => p
                   .LeftBindingPower(1)
                   .ProduceLeft((ctx, l, op) =>
                   {
                       var contents = ctx.Parse(0);
                       ctx.Expect(Match(']'));
                       return $"({l}[{contents}])";
                   })
               )
               .Add(Match('+'), p => p
                   .LeftBindingPower(1)
                   .ProduceLeft((ctx, l, op) =>
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
                .Add(DigitString(), p => p
                    .LeftBindingPower(0)
                    .ProduceRight((ctx, value) => value.Value)
                )
                .Add(Identifier(), p => p
                    .LeftBindingPower(0)
                    .ProduceRight((ctx, value) => value.Value)
                )
                .Add(Match('+'), p => p
                    .LeftBindingPower(3)
                    .ProduceLeft((ctx, left, op) =>
                    {
                        var right = ctx.Parse();
                        return $"({left}+{right})";
                    })
                )
                .Add(Match('='), p => p
                    .LeftBindingPower(2)
                    .RightBindingPower(1)
                    .ProduceLeft((ctx, left, op) =>
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

        //[Test]
        //public void GetChildren_Test()
        //{
        //    var number = Digit().Transform(c => c.ToString());
        //    var plus = Match('+');
        //    var neg = Match('-');
        //    var bang = Match('!');
        //    var oParen = Match('(');
        //    var cParen = Match(')');
        //    var oBracket = Match('[');
        //    var cBracket = Match(']');
        //    var target = Pratt<char, string>(number, config => config
        //        .AddInfix(plus, 1, 2, (_, _, _) => null)
        //        .AddPrefix(neg, 3, (_, _) => null)
        //        .AddPostfix(bang, 5, (_, _) => null)
        //        .AddCircumfix(oParen, cParen, (_, _, _) => null)
        //        .AddPostcircumfix(oBracket, cBracket, 7, (_, _, _, _) => null)
        //    );

        //    var children = target.GetChildren().ToList();
        //    children.Count.Should().Be(8);
        //    children.Should().Contain(number);
        //    children.Should().Contain(plus);
        //    children.Should().Contain(neg);
        //    children.Should().Contain(bang);
        //    children.Should().Contain(oParen);
        //    children.Should().Contain(cParen);
        //    children.Should().Contain(oBracket);
        //    children.Should().Contain(cBracket);
        //}
    }
}
