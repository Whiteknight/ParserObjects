using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Caching;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class CacheParserTests
    {
        [Test]
        public void Output_Method_Test()
        {
            var parser = Cache(CharacterString("TEST"));
            var input = new StringCharacterSequence("TEST");
            var start = input.Checkpoint();
            var cache = new MemoryCacheResultsCache();
            var state = new ParseState<char>(input, _ => { }, cache);

            // First attempt, we parse the string for the first time. There's nothing in cache
            var result = parser.Parse(state);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");
            var stats = cache.GetStatistics();
            stats.Attempts.Should().Be(1);
            stats.Misses.Should().Be(1);
            stats.Hits.Should().Be(0);

            // Second attempt, we start from the beginning again, but now the value is in cache
            // already. We keep the same statistics from last time (1 attempt, 1 miss) and append
            // the new hit
            start.Rewind();
            result = parser.Parse(state);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");
            stats = cache.GetStatistics();
            stats.Attempts.Should().Be(2);
            stats.Misses.Should().Be(1);
            stats.Hits.Should().Be(1);
        }
    }
}
