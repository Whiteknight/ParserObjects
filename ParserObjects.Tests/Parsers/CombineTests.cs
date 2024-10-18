using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class CombineTests
{
    public class MethodParams
    {
        [Test]
        public void Parse_Empty()
        {
            var parser = Combine();
            var input = FromString("abd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count.Should().Be(0);
        }
    }

    public class MethodList
    {
        [Test]
        public void Parse_Empty()
        {
            var parser = Combine(new List<IParser<char>>());
            var input = FromString("abd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count.Should().Be(0);
        }
    }

    public class TupleExtension
    {
        private readonly IParser<char, char> _any = Any();

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();

            var target = (_any, failParser).Combine();

            var result = target.GetChildren().ToList();
            result[0].Should().BeSameAs(_any);
            result[1].Should().BeSameAs(failParser);
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = (Match('a'), Match('b'), Match('c')).Combine();
            var input = FromString("abd");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }

        [Test]
        public void ValueTuple_Combine_2_Test()
        {
            var target = (_any, _any).Combine();

            var input = FromString("abc");

            target.Parse(input).Value.Count.Should().Be(2);
        }

        [Test]
        public void ValueTuple_Combine_3_Test()
        {
            var target = (_any, _any, _any).Combine();

            var input = FromString("abc");

            target.Parse(input).Value.Count.Should().Be(3);
        }

        [Test]
        public void ValueTuple_Combine_4_Test()
        {
            var target = (_any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(4);
        }

        [Test]
        public void ValueTuple_Combine_5_Test()
        {
            var target = (_any, _any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(5);
        }

        [Test]
        public void ValueTuple_Combine_6_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(6);
        }

        [Test]
        public void ValueTuple_Combine_7_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(7);
        }

        [Test]
        public void ValueTuple_Combine_8_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(8);
        }

        [Test]
        public void ValueTuple_Combine_9_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any, _any).Combine();

            var input = FromString("abcdefghi");

            var result = target.Parse(input).Value;
            result.Count.Should().Be(9);
        }

        [Test]
        public void ToBnf_2()
        {
            var target = (_any, _any).Combine().Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := (. .)");
        }
    }
}
