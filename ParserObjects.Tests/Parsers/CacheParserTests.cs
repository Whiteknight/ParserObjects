using static ParserObjects.Caches;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

namespace ParserObjects.Tests.Parsers
{
    public class CacheParserTests
    {
        [Test]
        public void Method_Parse_NoOutput()
        {
            var parser = Cache(End());
            var input = FromString("TEST");
            var start = input.Checkpoint();
            var cache = InMemoryCache();
            var state = new ParseState<char>(input, _ => { }, cache);

            // First attempt, we parse the string for the first time. There's nothing in cache
            var result = parser.Parse(state);
            result.Success.Should().BeFalse();
            var stats = cache.GetStatistics();
            stats.Attempts.Should().Be(1);
            stats.Misses.Should().Be(1);
            stats.Hits.Should().Be(0);

            // Second attempt, we start from the beginning again, but now the value is in cache
            // already. We keep the same statistics from last time (1 attempt, 1 miss) and append
            // the new hit
            start.Rewind();
            result = parser.Parse(state);
            result.Success.Should().BeFalse();
            stats = cache.GetStatistics();
            stats.Attempts.Should().Be(2);
            stats.Misses.Should().Be(1);
            stats.Hits.Should().Be(1);
        }

        [Test]
        public void Method_Parse_Output()
        {
            var parser = Cache(CharacterString("TEST"));
            var input = FromString("TEST");
            var start = input.Checkpoint();
            var cache = InMemoryCache();
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

        [Test]
        public void Method_Parse_MultiOutput()
        {
            var parser = Cache(ProduceMulti(() => new[] { "abc" }));
            var input = FromString("TEST");
            var start = input.Checkpoint();
            var cache = InMemoryCache();
            var state = new ParseState<char>(input, _ => { }, cache);

            // First attempt, we parse the string for the first time. There's nothing in cache
            var result = parser.Parse(state);
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("abc");
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
            result.Results[0].Value.Should().Be("abc");
            stats = cache.GetStatistics();
            stats.Attempts.Should().Be(2);
            stats.Misses.Should().Be(1);
            stats.Hits.Should().Be(1);
        }

        [Test]
        public void ToBnf_NoValue()
        {
            var parser = Cache(End());
            var result = parser.ToBnf();
            result.Should().Contain("(TARGET) := CACHED(END)");
        }

        [Test]
        public void ToBnf_SingleValue()
        {
            var parser = Cache(CharacterString("TEST"));
            var result = parser.ToBnf();
            result.Should().Contain("(TARGET) := CACHED('T' 'E' 'S' 'T')");
        }

        [Test]
        public void ToBnf_MultiValue()
        {
            var parser = Cache(ProduceMulti(() => new[] { "abc" }));
            var result = parser.ToBnf();
            result.Should().Contain("(TARGET) := CACHED(PRODUCE)");
        }
    }
}
