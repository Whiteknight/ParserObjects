using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class ListTests
{
    public class UnseparatedMethod
    {
        [Test]
        public void Parse_NotAtLeastOne()
        {
            var parser = List(Any(), false);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value.ToList();
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_None_AtLeastOneFalse()
        {
            var parser = List(Match(char.IsNumber), false);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_AtLeastOneFalse()
        {
            var anyParser = Any();
            var parser = List(anyParser, false);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void Parse_Test()
        {
            var parser = List(Any());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value;
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_Minimum_Fail()
        {
            var parser = List(Any(), 4);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Exactly_Success()
        {
            var parser = List(Any(), 4, 4);
            var input = FromString("abcd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(4);
        }

        [Test]
        public void Parse_Exactly_TooFew()
        {
            var parser = List(Any(), 4, 4);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Maximum()
        {
            var parser = List(Any(), 0, 2);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(2);
            var list = result.Value;
            list.Count.Should().Be(2);
            list[0].Should().Be('a');
            list[1].Should().Be('b');
        }

        [Test]
        public void Parse_None()
        {
            var parser = List(Match(char.IsNumber));
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = List(Produce(() => new object()));
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var list = result.Value;
            list.Count.Should().Be(1);
            list[0].Should().NotBeNull();
        }

        [Test]
        public void Parse_Empty_Minimum()
        {
            var parser = List(Produce(() => new object()), 3);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var list = result.Value;
            list.Count.Should().Be(3);
            list[0].Should().NotBeNull();
            list[1].Should().NotBeNull();
            list[2].Should().NotBeNull();
        }

        [Test]
        public void Parse_Fail_DoesNotBacktrack()
        {
            // Test to show that List() is greedy and does not backtrack. This helps to contrast
            // it against NonGreedyList()
            var target = Rule(
                List(Match('a')),
                CharacterString("ab"),
                (l, r) => $"{l}{r}"
            );
            var result = target.Parse("aaab");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var parser = List(anyParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }
    }

    public class UnseparatedExtension
    {
    }

    public class SeparatedMethod
    {
        [Test]
        public void Parse_Test()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: false
            );
            var input = FromString("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
            result.Consumed.Should().Be(7);
        }

        [Test]
        public void Parse_TrailingSeparator()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: false
            );
            var input = FromString("1,2,3,4,");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
            result.Consumed.Should().Be(7);
        }

        [Test]
        public void ListSeparatedBy_Parse_Test()
        {
            var parser = Integer()
                .ListSeparatedBy(
                    Match(","),
                    atLeastOne: false
                );
            var input = FromString("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: false
            );
            var input = FromString("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_AtLeastOne_HasOne()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: true
            );
            var input = FromString("1");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
        }

        [Test]
        public void Parse_AtLeastOne_Multiple()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: true
            );
            var input = FromString("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
        }

        [Test]
        public void Parse_AtLeastOne_Empty()
        {
            var parser = List(
                Integer(),
                Match(","),
                atLeastOne: true
            );
            var input = FromString("");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Minimum_Fail()
        {
            var parser = List(
                Integer(),
                Match(","),
                4
            );
            var input = FromString("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Maximum()
        {
            var parser = List(
                Integer(),
                Match(","),
                0,
                2
            );
            var input = FromString("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value.Count.Should().Be(2);
            value[0].Should().Be(1);
            value[1].Should().Be(2);
        }
    }

    public class SeparatedExtension
    {
    }
}
