using BenchmarkDotNet.Attributes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.BenchmarkHarness;

[MemoryDiagnoser]
public class CIntegerBenchmarks
{
    private const string _text1 = "-12345678";
    private const string _text2 = "12345678";
    private const string _text3 = "0";

    // This is the original implementation
    [Benchmark(Baseline = true)]
    public void OriginalImplementation()
    {
        var maybeMinus = MatchChar('-').Transform(c => "-").Optional(() => string.Empty);
        var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
        var digits = Digit().ListCharToString();
        var zero = MatchChar('0').Transform(c => "0");
        var nonZeroNumber = (maybeMinus, nonZeroDigit, digits)
            .Rule((sign, start, body) => sign + start + body);
        var parser = (nonZeroNumber, zero).First();
        parser.Parse(_text1);
        parser.Parse(_text2);
        parser.Parse(_text3);
    }

    // This is the "current" implementation using Capture()
    [Benchmark]
    public void CaptureBasedImplementation()
    {
        var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
        var digits = Digit().List();
        var parser = First(
            Capture(
                MatchChar('-').Optional(),
                nonZeroDigit,
                digits
            ).Transform(c => new string(c)),
            MatchChar('0').Transform(_ => "0")
        );
        parser.Parse(_text1);
        parser.Parse(_text2);
        parser.Parse(_text3);
    }
}
