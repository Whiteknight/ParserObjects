using BenchmarkDotNet.Attributes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.BenchmarkHarness;

[MemoryDiagnoser]
public class CaptureBenchmarks
{
    private const string _text = "/* TEST */";

    [Benchmark]
    public void RuleBasedParser()
    {
        var start = Match("/*").Transform(c => "/*");
        var end = Match("*/").Transform(c => "*/");
        var standaloneAsterisk = MatchChar('*').NotFollowedBy(MatchChar('/'));
        var notAsterisk = Match(c => c != '*');

        var bodyChar = (standaloneAsterisk, notAsterisk).First();
        var bodyChars = bodyChar.ListCharToString();

        var parser = (start, bodyChars, end)
            .Rule((s, b, e) => s + b + e);

        var result = parser.Parse(_text);
    }

    [Benchmark]
    public void CaptureBasedParser()
    {
        var standaloneAsterisk = MatchChar('*').NotFollowedBy(MatchChar('/'));
        var notAsterisk = Match(c => c != '*');
        var bodyChar = (standaloneAsterisk, notAsterisk).First();

        var parser = Capture(
            And(
                Match("/*"),
                bodyChar.List(),
                Match("*/")
            )
        );

        var result = parser.Parse(_text).Transform(c => new string(c));
    }
}
