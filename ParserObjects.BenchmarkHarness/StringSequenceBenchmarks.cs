using BenchmarkDotNet.Attributes;
using ParserObjects.Internal.Sequences;
using static ParserObjects.Parsers;

namespace ParserObjects.BenchmarkHarness;

public class StringSequenceBenchmarks
{
    private readonly IParser<char, string> _parser = Regex("a*aaaaaaaaab");

    [Benchmark]
    public void Baseline()
    {
        var target = new StringCharacterSequence("aaaaaaaaaaaaaaaaaaab");
        var result = _parser.Parse(target);
    }

    [Benchmark]
    public void Prenormalized()
    {
        var target = new PrenormalizedStringCharacterSequence("aaaaaaaaaaaaaaaaaaab");
        var result = _parser.Parse(target);
    }

    [Benchmark]
    public void Nonnormalized()
    {
        var target = new NonnormalizedStringCharacterSequence("aaaaaaaaaaaaaaaaaaab");
        var result = _parser.Parse(target);
    }
}
