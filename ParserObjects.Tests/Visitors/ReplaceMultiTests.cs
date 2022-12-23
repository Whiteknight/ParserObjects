using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors
{
    public class ReplaceMultiTests
    {
        [Test]
        public void ReplaceMulti_ByName()
        {
            var fail = FailMulti<char>();
            var needle = FailMulti<char>().Replaceable().Named("needle");
            var success = ProduceMulti(() => new[] { 'X' });
            var haystack = needle;

            var parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeFalse();

            var result = haystack.ReplaceMulti<char, char>("needle", x => success);
            result.Success.Should().BeTrue();

            parseResult = haystack.Parse("X");
            parseResult.Success.Should().BeTrue();
            parseResult.Results[0].Value.Should().Be('X');
        }
    }
}
