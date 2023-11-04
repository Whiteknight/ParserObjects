using System.Collections.Generic;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class StringifyTests
{
    public class Array
    {
        [Test]
        public void Test()
        {
            var target = Stringify(Produce(() => new[] { 'a', 'b', 'c' }));
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }
    }

    public class List
    {
        [Test]
        public void Test()
        {
            var target = Stringify(Produce(() => new List<char> { 'a', 'b', 'c' }));
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }
    }
}
