using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class ReplaceableTests
{
    public class SingleOutputFunc
    {
        [Test]
        public void Parse_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail()
        {
            var anyParser = Fail<char>();
            var target = Replaceable(anyParser);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Null()
        {
            var target = Replaceable((IParser<char, char>)null);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void SetParser_Test()
        {
            var anyParser = Any();
            var failParser = Fail<char>();
            var target = Replaceable(failParser);
            (target as IReplaceableParserUntyped)?.SetParser(anyParser);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Replaceable(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }
    }

    public class SingleOutputExtension
    {
        [Test]
        public void Parse_Test()
        {
            var anyParser = Any();
            var target = anyParser.Replaceable();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }
    }

    public class SingleNoOutputFunc
    {
        [Test]
        public void Parse_Test()
        {
            var target = Replaceable(End());
            var input = FromString("");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void GetChildren_Test()
        {
            var end = End();
            var target = Replaceable(end);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(end);
        }

        [Test]
        public void Parse_Null()
        {
            var target = Replaceable((IParser<char>)null);
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void SetParser_Test()
        {
            var startParser = End();
            var replaceParser = startParser.Named("replaced");
            var target = Replaceable(startParser);
            var result = (target as IReplaceableParserUntyped)?.SetParser(replaceParser);

            var child = target.GetChildren().First();
            child.Should().BeSameAs(replaceParser);
            child.Should().NotBeSameAs(startParser);

            result.Value.Success.Should().BeTrue();
            var (success, old, newp, parent) = result.Value;
            success.Should().BeTrue();
            old.Should().BeSameAs(startParser);
            newp.Should().BeSameAs(replaceParser);
            parent.Should().BeSameAs(target);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Replaceable(End()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := END");
        }
    }

    public class SingleNoOutputExtension
    {
        [Test]
        public void Parse_Test()
        {
            var target = End().Replaceable();
            var input = FromString("");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
        }
    }

    public class MultiFunc
    {
        [Test]
        public void ToBnf_Test()
        {
            var parser = Replaceable(ProduceMulti(() => new char[0])).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }
    }

    public class MultiExtension
    {
        [Test]
        public void ToBnf_Test()
        {
            var parser = ProduceMulti(() => new char[0]).Replaceable().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }
    }
}
