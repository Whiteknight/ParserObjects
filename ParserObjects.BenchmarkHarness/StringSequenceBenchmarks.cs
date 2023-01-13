using BenchmarkDotNet.Attributes;
using ParserObjects.Internal.Sequences;
using static ParserObjects.Parsers;

namespace ParserObjects.BenchmarkHarness;

public class StringSequenceBenchmarks
{
    private readonly IParser<char, string> _parser = Regex("a*aaaaaaaaab");

    [Benchmark]
    public void Prenormalized()
    {
        var target = new CharBufferSequence.FromCharArray("aaaaaaaaaaaaaaaaaaab", new SequenceOptions<char>
        {
            MaintainLineEndings = false
        });
        var result = _parser.Parse(target);
    }

    [Benchmark]
    public void Nonnormalized()
    {
        var target = new CharBufferSequence.FromNonnormalizedString("aaaaaaaaaaaaaaaaaaab");
        var result = _parser.Parse(target);
    }
}
