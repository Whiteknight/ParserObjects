using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.ParserMethods;
using FluentAssertions;
using System.Linq;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class PrattParserTests
    {
        [Test]
        public void SingleNumber()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void SingleNumber_Remainder()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var input = new StringCharacterSequence("1a");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Infix_Addition()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("1+2");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1+2)");
        }

        [Test]
        public void Infix_AdditionSubtractionChain()
        {
            // In most C-like languages and others, +/- have the same precidence and are 
            // left associative
            var number = Digit().Transform(c => c.ToString());

            var target = Pratt<char, string>(number, config => config
                .AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})")
                .AddInfixOperator(Match('-'), 1, 2, (l, op, r) => $"({l}{op}{r})")
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
            var number = Digit().Transform(c => c.ToString());

            var target = Pratt<char, string>(number, config => config
                .AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})")
                .AddInfixOperator(Match('-'), 1, 2, (l, op, r) => $"({l}{op}{r})")
                .AddInfixOperator(Match('*'), 3, 4, (l, op, r) => $"({l}{op}{r})")
                .AddInfixOperator(Match('/'), 3, 4, (l, op, r) => $"({l}{op}{r})")
            );
            var result = target.Parse("1+2*3+4/5");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("((1+(2*3))+(4/5))");
        }

        [Test]
        public void Infix_EqualsChain()
        {
            // In most C-like languages and others, = is right-associative
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddInfixOperator(Match('='), 2, 1, (l, op, r) => $"({l}{op}{r})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("1=2=3=4");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1=(2=(3=4)))");
        }

        [Test]
        public void Prefix_Negation()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddPrefixOperator(Match('-'), 1, (op, r) => $"({op}{r})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("-1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(-1)");
        }

        [Test]
        public void Postfix_Factorial()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddPostfixOperator(Match('!'), 1, (l, op) => $"({l}{op})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("1!");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1!)");
        }

        [Test]
        public void Circumfix_NestedParens()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddCircumfixOperator(Match('('), Match(')'), (s, v, e) => $"{s}{v}{e}");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("(((1)))");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(((1)))");
        }

        [Test]
        public void Circumfix_Brackets()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})");
            config.AddCircumfixOperator(Match('['), Match(']'), (s, v, e) => $"({s}{v}{e})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("[1+2]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("([(1+2)])");
        }

        [Test]
        public void Postcircumfix_BracketsIndex()
        {
            var config = Pratt<char, char, string>.CreateConfiguration();
            config.AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})");
            config.AddPostcircumfixOperator(Match('['), Match(']'), 3, (l, s, r, e) => $"({l}{s}{r}{e})");
            var number = Digit().Transform(c => c.ToString());

            var target = new Pratt<char, char, string>.Parser(number, config);
            var result = target.Parse("1[2+3]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1[(2+3)])");
        }

        [Test]
        public void ParserMethod_Postcircumfix_BracketsIndex()
        {
            var number = Digit().Transform(c => c.ToString());
            var target = Pratt<char, string>(number, config => config
                .AddInfixOperator(Match('+'), 1, 2, (l, op, r) => $"({l}{op}{r})")
                .AddPostcircumfixOperator(Match('['), Match(']'), 3, (l, s, r, e) => $"({l}{s}{r}{e})")
            );
            
            var result = target.Parse("1[2+3]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("(1[(2+3)])");
        }

        [Test]
        public void GetChildren_Test()
        {
            var number = Digit().Transform(c => c.ToString());
            var plus = Match('+');
            var neg = Match('-');
            var bang = Match('!');
            var oParen = Match('(');
            var cParen = Match(')');
            var oBracket = Match('[');
            var cBracket = Match(']');
            var target = Pratt<char, string>(number, config => config
                .AddInfixOperator(plus, 1, 2, (_, _, _) => null)
                .AddPrefixOperator(neg, 3, (_, _) => null)
                .AddPostfixOperator(bang, 5, (_, _) => null)
                .AddCircumfixOperator(oParen, cParen, (_, _, _) => null)
                .AddPostcircumfixOperator(oBracket, cBracket, 7, (_, _, _, _) => null)
            );

            var children = target.GetChildren().ToList();
            children.Count.Should().Be(8);
            children.Should().Contain(number);
            children.Should().Contain(plus);
            children.Should().Contain(neg);
            children.Should().Contain(bang);
            children.Should().Contain(oParen);
            children.Should().Contain(cParen);
            children.Should().Contain(oBracket);
            children.Should().Contain(cBracket);
        }
    }
}
