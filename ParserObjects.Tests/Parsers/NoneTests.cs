﻿using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class NoneTests
{
    public class Method
    {
        [Test]
        public void Parse_Output_Test()
        {
            var target = None(Any());
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Output_Fail()
        {
            var target = None(Fail<char>());
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Test()
        {
            var target = None(And(Any(), Any()));
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = None(And(Any(), Fail<char>()));
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = None(Any()).Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := (?=.)");
        }
    }

    public class Extension
    {
        [Test]
        public void Parse_Output_Test()
        {
            var target = Any().None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Output_Fail()
        {
            var target = Fail<char>().None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Test()
        {
            var target = And(Any(), Any()).None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = And(Any(), Fail<char>()).None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Any().None().Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := (?=.)");
        }
    }
}