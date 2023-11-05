using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class ListTests
{
    public class TypedUnseparatedMethod
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
            result.Count.Should().Be(2);
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
            result.Count.Should().Be(2);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void ToBnf_NoBounds()
        {
            var parser = List(Any()).Named("SUT");
            parser.ToBnf().Should().Contain("SUT := .*");
        }

        [Test]
        public void ToBnf_AtLeastOne()
        {
            var parser = Any().List(true).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .+");
        }

        [Test]
        public void ToBnf_Max()
        {
            var parser = Any().List(maximum: 5).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .{0, 5}");
        }

        [Test]
        public void ToBnf_Min()
        {
            var parser = Any().List(5).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .{5,}");
        }

        [TestCase(0, 1, ".?")]
        [TestCase(4, 4, ".{4}")]
        [TestCase(0, 10, ".{0, 10}")]
        [TestCase(0, null, ".*")]
        [TestCase(1, null, ".+")]
        [TestCase(3, null, ".{3,}")]
        public void ToBnf_MinMax(int min, int? max, string pattern)
        {
            var parser = List(Any(), minimum: min, maximum: max).Named("SUT");
            parser.ToBnf().Should().Contain($"SUT := {pattern}");
        }
    }

    public class UntypedUnseparatedMethod
    {
        [Test]
        public void Parse_NotAtLeastOne()
        {
            var parser = List((IParser<char>)Any(), false);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value as List<object>;
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_None_AtLeastOneFalse()
        {
            var parser = List((IParser<char>)Match(char.IsNumber), false);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            (result.Value as List<object>).Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_AtLeastOneFalse()
        {
            var anyParser = (IParser<char>)Any();
            var parser = List(anyParser, false);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(2);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void Parse_Test()
        {
            var parser = List((IParser<char>)Any());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value as List<object>;
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_Minimum_Fail()
        {
            var parser = List((IParser<char>)Any(), 4);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Exactly_Success()
        {
            var parser = List((IParser<char>)Any(), 4, 4);
            var input = FromString("abcd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(4);
        }

        [Test]
        public void Parse_Exactly_TooFew()
        {
            var parser = List((IParser<char>)Any(), 4, 4);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Maximum()
        {
            var parser = List((IParser<char>)Any(), 0, 2);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(2);
            var list = result.Value as List<object>;
            list.Count.Should().Be(2);
            list[0].Should().Be('a');
            list[1].Should().Be('b');
        }

        [Test]
        public void Parse_None()
        {
            var parser = List((IParser<char>)Match(char.IsNumber));
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            (result.Value as List<object>).Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = List((IParser<char>)Produce(() => new object()));
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var list = result.Value as List<object>;
            list.Count.Should().Be(1);
            list[0].Should().NotBeNull();
        }

        [Test]
        public void Parse_Empty_Minimum()
        {
            var parser = List((IParser<char>)Produce(() => new object()), 3);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var list = result.Value as List<object>;
            list.Count.Should().Be(3);
            list[0].Should().NotBeNull();
            list[1].Should().NotBeNull();
            list[2].Should().NotBeNull();
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = (IParser<char>)Any();
            var parser = List(anyParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(2);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = List((IParser<char>)Any()).Named("SUT");
            parser.ToBnf().Should().Contain("SUT := .*");
        }

        [TestCase(0, 1, ".?")]
        [TestCase(4, 4, ".{4}")]
        [TestCase(0, 10, ".{0, 10}")]
        [TestCase(0, null, ".*")]
        [TestCase(1, null, ".+")]
        [TestCase(3, null, ".{3,}")]
        public void ToBnf_MinMax(int min, int? max, string pattern)
        {
            var parser = List((IParser<char>)Any(), minimum: min, maximum: max).Named("SUT");
            parser.ToBnf().Should().Contain($"SUT := {pattern}");
        }
    }

    public class TypedUnseparatedExtension
    {
    }

    public class UntypedUnseparatedExtension
    {
        [Test]
        public void Parse_AtLeastOneTrue()
        {
            var parser = ((IParser<char>)Any()).List(true);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value as List<object>;
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_Maximum()
        {
            var parser = ((IParser<char>)Any()).List(0, 2);
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(2);
            var list = result.Value as List<object>;
            list.Count.Should().Be(2);
            list[0].Should().Be('a');
            list[1].Should().Be('b');
        }
    }

    public class TypedSeparatedMethod
    {
        [Test]
        public void Parse_Test()
        {
            var parser = List(
                Integer(),
                MatchChar(','),
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
                MatchChar(','),
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
        public void Parse_Empty()
        {
            var parser = List(
                Integer(),
                MatchChar(','),
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
                MatchChar(','),
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
                MatchChar(','),
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
                MatchChar(','),
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
                MatchChar(','),
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
                MatchChar(','),
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

        [Test]
        public void ToBnf_Test()
        {
            var parser = List(MatchChar('a'), MatchChar('X')).Named("SUT");
            parser.ToBnf().Should().Contain("SUT := 'a' ('X' 'a')*");
        }

        [TestCase(0, 1, "'a' ('X' 'a')?")]
        [TestCase(4, 4, "'a' ('X' 'a'){4}")]
        [TestCase(0, 10, "'a' ('X' 'a'){0, 10}")]
        [TestCase(0, null, "'a' ('X' 'a')*")]
        [TestCase(1, null, "'a' ('X' 'a')+")]
        [TestCase(3, null, "'a' ('X' 'a'){3,}")]
        public void ToBnf_MinMax(int min, int? max, string pattern)
        {
            var parser = List(MatchChar('a'), MatchChar('X'), minimum: min, maximum: max).Named("SUT");
            parser.ToBnf().Should().Contain($"SUT := {pattern}");
        }
    }

    public class UntypedSeparatedMethod
    {
        [Test]
        public void Parse_Test()
        {
            var parser = List(
                (IParser<char>)Integer(),
                MatchChar(','),
                atLeastOne: false
            );
            var input = FromString("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
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
                (IParser<char>)Integer(),
                MatchChar(','),
                atLeastOne: false
            );
            var input = FromString("1,2,3,4,");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
            result.Consumed.Should().Be(7);
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = List(
                (IParser<char>)Integer(),
                MatchChar(','),
                atLeastOne: false
            );
            var input = FromString("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            (result.Value as List<object>).Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_AtLeastOne_HasOne()
        {
            var parser = List(
                (IParser<char>)Integer(),
                MatchChar(','),
                atLeastOne: true
            );
            var input = FromString("1");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
            value[0].Should().Be(1);
        }

        [Test]
        public void Parse_AtLeastOne_Multiple()
        {
            var parser = List(
                (IParser<char>)Integer(),
                MatchChar(','),
                atLeastOne: true
            );
            var input = FromString("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
        }

        [Test]
        public void Parse_AtLeastOne_Empty()
        {
            var parser = List(
                (IParser<char>)Integer(),
                MatchChar(','),
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
                (IParser<char>)Integer(),
                MatchChar(','),
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
                (IParser<char>)Integer(),
                MatchChar(','),
                0,
                2
            );
            var input = FromString("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
            value.Count.Should().Be(2);
            value[0].Should().Be(1);
            value[1].Should().Be(2);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = List((IParser<char>)MatchChar('a'), MatchChar('X')).Named("SUT");
            parser.ToBnf().Should().Contain("SUT := 'a' ('X' 'a')*");
        }

        [TestCase(0, 1, "'a' ('X' 'a')?")]
        [TestCase(4, 4, "'a' ('X' 'a'){4}")]
        [TestCase(0, 10, "'a' ('X' 'a'){0, 10}")]
        [TestCase(0, null, "'a' ('X' 'a')*")]
        [TestCase(1, null, "'a' ('X' 'a')+")]
        [TestCase(3, null, "'a' ('X' 'a'){3,}")]
        public void ToBnf_MinMax(int min, int? max, string pattern)
        {
            var parser = List((IParser<char>)MatchChar('a'), MatchChar('X'), minimum: min, maximum: max).Named("SUT");
            parser.ToBnf().Should().Contain($"SUT := {pattern}");
        }
    }

    public class TypedSeparatedExtension
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Integer()
                .List(
                    MatchChar(','),
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
    }

    public class UntypedSeparatedExtension
    {
        [Test]
        public void Parse_Test()
        {
            var parser = ((IParser<char>)Integer())
                .List(
                    MatchChar(','),
                    atLeastOne: false
                );
            var input = FromString("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value as List<object>;
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
        }
    }
}
