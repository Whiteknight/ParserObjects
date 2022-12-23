using ParserObjects.Internal.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors
{
    public class FindOfTypeTests
    {
        [Test]
        public void OfType_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = haystack.FindOfType<FailParser<char, char>>();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(needle);
        }
    }
}
