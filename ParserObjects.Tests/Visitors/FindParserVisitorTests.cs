using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Visitors;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Visitors
{
    public class FindParserVisitorTests
    {
        [Test]
        public void Named_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Named(haystack, "needle");
            result.Success.Should().BeTrue();
            result.Value.Should().BeSameAs(needle);
        }

        [Test]
        public void FindNamed_Extension_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = haystack.FindNamed("needle");
            result.Success.Should().BeTrue();
            result.Value.Should().BeSameAs(needle);
        }

        [Test]
        public void OfType_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.OfType<FailParser<char, char>>(haystack);
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(needle);
        }

        [Test]
        public void Replace_Fail_RootNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(null, _ => false, needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_PredicateNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, (Func<IReplaceableParserUntyped, bool>)null, needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, r => false, null);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_RootNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(null, _ => false, _ => needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_PredicateNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, (Func<IReplaceableParserUntyped, bool>)null, _ => needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Replaceable().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, _ => false, null);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Single_ByName()
        {
            var fail = Fail<char>();
            var needle = Fail<char>().Replaceable().Named("needle");
            var success = Any();
            var haystack = (fail, fail, fail, needle).First();

            var parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeFalse();

            var result = FindParserVisitor.Replace(haystack, "needle", success);
            result.Success.Should().BeTrue();

            parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeTrue();
            parseResult.Value.Should().Be('X');
        }

        [Test]
        public void ReplaceMulti_ByName()
        {
            var fail = FailMulti<char>();
            var needle = FailMulti<char>().Replaceable().Named("needle");
            var success = ProduceMulti(() => new[] { 'X' });
            var haystack = needle;

            var parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeFalse();

            var result = FindParserVisitor.ReplaceMulti<char, char>(haystack, "needle", x => success);
            result.Success.Should().BeTrue();

            parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeTrue();
            parseResult.Results[0].Value.Should().Be('X');
        }
    }
}
