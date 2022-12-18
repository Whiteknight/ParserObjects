using BenchmarkDotNet.Attributes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.BenchmarkHarness;

[MemoryDiagnoser]
public class CaptureBenchmarks
{
    private const string _text = "/* TEST */";

    [Benchmark(Baseline = true)]
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

        var result = parser.Parse(_text, new SequenceOptions<char> { MaintainLineEndings = true });
    }

    [Benchmark]
    public void CaptureBasedParser()
    {
        var standaloneAsterisk = MatchChar('*').NotFollowedBy(MatchChar('/'));
        var notAsterisk = Match(c => c != '*');
        var bodyChar = (standaloneAsterisk, notAsterisk).First();

        var parser = Capture(
            Match("/*"),
            bodyChar.List(),
            Match("*/")
        ).Transform(c => new string(c));

        var result = parser.Parse(_text, new SequenceOptions<char> { MaintainLineEndings = true });
    }

    [Benchmark]
    public void CombineBasedParser()
    {
        var standaloneAsterisk = MatchChar('*').NotFollowedBy(MatchChar('/'));
        var notAsterisk = Match(c => c != '*');
        var bodyChar = (standaloneAsterisk, notAsterisk).First();

        var parser = Combine(
            Match("/*"),
            bodyChar.ListCharToString(),
            Match("*/")
        ).Transform(o => "/*" + ((string)o[1]) + "*/");

        var result = parser.Parse(_text, new SequenceOptions<char> { MaintainLineEndings = true });
    }

    [Benchmark]
    public void SequenceBasedParser()
    {
        var parser = Sequential(s =>
        {
            s.Parse(MatchChar('/'));
            s.Parse(MatchChar('*'));

            var chars = new List<char>
            {
                '/',
                '*'
            };

            while (!s.Input.IsAtEnd)
            {
                var c = s.Input.GetNext();
                chars.Add(c);
                if (c == '*')
                {
                    var lookahead = s.Input.Peek();
                    if (lookahead == '/')
                    {
                        s.Input.GetNext();
                        chars.Add(lookahead);

                        return new string(chars.ToArray());
                    }
                }
            }

            s.Fail("Could not find */");
            return "";
        });

        var result = parser.Parse(_text, new SequenceOptions<char> { MaintainLineEndings = true });
    }
}
