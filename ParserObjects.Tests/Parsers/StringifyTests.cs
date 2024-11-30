using System.Collections.Generic;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class StringifyTests
{
    public class ListOfChars
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

    public class ArrayExtension
    {
        [Test]
        public void Test()
        {
            var target = Produce(() => new[] { 'a', 'b', 'c' }).Stringify();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }
    }

    public class ListExtension
    {
        [Test]
        public void Test()
        {
            var target = Produce(() => (IReadOnlyList<char>)new List<char> { 'a', 'b', 'c' }).Stringify();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }
    }
}
