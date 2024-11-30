using System.IO;
using System.Text;
using ParserObjects.Regexes;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class RegexTests
{
    [Test]
    public void Regex_EmptyPattern()
    {
        var parser = Regex(string.Empty);
        var result = parser.Parse("abc");
        result.Success.Should().BeTrue();
        result.Value.Should().Be(string.Empty);
    }

    [Test]
    public void Regex_NullPattern()
    {
        var parser = Regex(null);
        var result = parser.Parse("abc");
        result.Success.Should().BeTrue();
        result.Value.Should().Be(string.Empty);
    }

    [Test]
    public void Regex_Parsers_EmptyPattern()
    {
        var a = MatchChar('a').Named("a");
        var target = Regex("", a);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    public void Regex_Parsers_NullPattern()
    {
        var a = MatchChar('a').Named("a");
        var target = Regex(null, a);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    public void RegexMatch_EmptyPattern()
    {
        var target = RegexMatch("");
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value[0][0].Should().Be("");
    }

    [Test]
    public void RegexMatch_NullPattern()
    {
        var target = RegexMatch(null);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value[0][0].Should().Be("");
    }

    [Test]
    public void RegexMatch_Parsers_EmptyPattern()
    {
        var a = MatchChar('a').Named("a");
        var target = RegexMatch("", a);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value[0][0].Should().Be("");
    }

    [Test]
    public void RegexMatch_Parsers_NullPattern()
    {
        var a = MatchChar('a').Named("a");
        var target = RegexMatch(null, a);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value[0][0].Should().Be("");
    }

    [Test]
    public void MatchLiteral_CanReparse()
    {
        // Test that we can reuse the parser, that the act of parsing doesn't destroy state
        var parser = Regex("abc");
        var result = parser.Parse("abcdef");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");

        result = parser.Parse("abcdef");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
    }

    [Test]
    public void Combines_Nicely()
    {
        var parser = Rule(
            Match('['),
            Regex("abc*"),
            Match(']'),
            (_, match, _) => match
        );
        var result = parser.Parse("[abcccccc]");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abcccccc");
    }

    private static void RegexTest(string pattern, string input, string expectedMatch)
    {
        var stringParser = Regex(pattern);
        var matchParser = RegexMatch(pattern);

        // First test with a string sequence
        var stringSequence = FromString(input, new SequenceOptions<char>
        {
            MaintainLineEndings = true
        });
        RegexTest(expectedMatch, stringParser, stringSequence);
        stringSequence.Reset();
        RegexTest(expectedMatch, matchParser, stringSequence);

        // Second test with a stream sequence, just to show that they are equivalent
        var bytes = Encoding.UTF8.GetBytes(input);
        using var memoryStream = new MemoryStream(bytes);
        var streamSequence = FromCharacterStream(memoryStream, new SequenceOptions<char>
        {
            MaintainLineEndings = true
        });
        RegexTest(expectedMatch, stringParser, streamSequence);
        streamSequence.Reset();
        RegexTest(expectedMatch, matchParser, streamSequence);
    }

    private static void RegexTest(string expectedMatch, IParser<char, string> parser, ISequence<char> sequence)
    {
        var result = parser.Parse(sequence);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expectedMatch);
        result.Consumed.Should().Be(expectedMatch.Length);
        sequence.Consumed.Should().Be(expectedMatch.Length);
    }

    private static void RegexTest(string expectedMatch, IParser<char, RegexMatch> parser, ISequence<char> sequence)
    {
        var result = parser.Parse(sequence);
        result.Success.Should().BeTrue();
        result.Value.GetCapture(0, 0).Should().Be(expectedMatch);
        result.Consumed.Should().Be(expectedMatch.Length);
        sequence.Consumed.Should().Be(expectedMatch.Length);
    }

    private static void RegexTestThrow(string pattern)
    {
        Action act = () => Regex(pattern);
        act.Should().Throw<RegexException>();
    }

    private static void RegexTestFail(string pattern, string input)
    {
        var sequence = FromString(input);
        var stringParser = Regex(pattern);
        var result1 = stringParser.Parse(sequence);
        result1.Success.Should().BeFalse();
        result1.Consumed.Should().Be(0);
        sequence.Consumed.Should().Be(0);

        var matchParser = RegexMatch(pattern);
        var result2 = matchParser.Parse(sequence);
        result2.Success.Should().BeFalse();
        result2.Consumed.Should().Be(0);
        sequence.Consumed.Should().Be(0);
    }

    [TestCase("abc", "abcd", "abc")]
    public void MatchLiterals(string pattern, string input, string expectedMatch)
        => RegexTest(pattern, input, expectedMatch);

    [TestCase("a", "bad")]
    public void MatchLiterals_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("ab?c", "abcd", "abc")]
    [TestCase("ab?c", "acd", "ac")]
    [TestCase("ab?", "a", "a")]
    [TestCase("ab?", "abbb", "ab")]
    [TestCase("a?b", "b", "b")]
    [TestCase("a?b", "ab", "ab")]
    public void ZeroOrOne(string pattern, string input, string expectedMatch)
         => RegexTest(pattern, input, expectedMatch);

    [TestCase("ab*c", "abbbbcd", "abbbbc")]
    [TestCase("ab*c", "acd", "ac")]
    [TestCase("ab*b", "abcd", "ab")]
    [TestCase("ab*b", "abbbbcd", "abbbb")]
    public void ZeroOrMore(string pattern, string input, string expectedMatch)
         => RegexTest(pattern, input, expectedMatch);

    [TestCase("ab+c", "abbbbcd", "abbbbc")]
    [TestCase("ab+c", "abcd", "abc")]
    public void OneOrMore(string pattern, string input, string expectedMatch)
         => RegexTest(pattern, input, expectedMatch);

    [TestCase("ab+c", "acde")]
    public void OneOrMore_Fail(string pattern, string input)
         => RegexTestFail(pattern, input);

    [TestCase("a(bc)?bc", "abcbcf", "abcbc")]
    [TestCase("a(bc)?bc", "abcdef", "abc")]
    [TestCase("a(bc)*bc", "abcbcbcdef", "abcbcbc")]
    public void Groups(string pattern, string input, string expectedMatch)
         => RegexTest(pattern, input, expectedMatch);

    [TestCase("a$(b)")]
    public void Groups_Throw(string pattern)
         => RegexTestThrow(pattern);

    [TestCase("\\d", "1a", "1")]
    [TestCase("\\D", "a1", "a")]
    [TestCase("\\w", "a ", "a")]
    [TestCase("\\W", " a", " ")]
    [TestCase("\\s", " a", " ")]
    [TestCase("\\S", "a ", "a")]
    [TestCase("(\\w+\\s)*\\w+", "this is a test.", "this is a test")]
    public void SpecialClasses(string pattern, string input, string expectedMatch)
         => RegexTest(pattern, input, expectedMatch);

    [TestCase("\\d", "a1")]
    [TestCase("\\D", "1a")]
    [TestCase("\\w", " a")]
    [TestCase("\\W", "a ")]
    [TestCase("\\s", "a ")]
    [TestCase("\\S", " a")]
    public void SpecialClasses_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("\\")]
    public void Escapes_Throw(string pattern)
        => RegexTestThrow(pattern);

    [TestCase(".*", "abcd+=.", "abcd+=.")]
    [TestCase("\\.", ".", ".")]
    [TestCase(".*\\..*", "command.com", "command.com")]
    public void Wildcard(string pattern, string input, string expectedMatch)
        => RegexTest(pattern, input, expectedMatch);

    [TestCase("\\.", "a")]
    public void Wildcard_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("abc$", "abc", "abc")]
    [TestCase("abc\\$", "abc$def", "abc$")]
    [TestCase("abc|abd$|abe", "abd", "abd")]
    public void End(string pattern, string input, string expectedMatch)
        => RegexTest(pattern, input, expectedMatch);

    [TestCase("abc$", "abcd")]
    [TestCase("abc|abd$|abe", "abde")]
    [TestCase("$", "abc")]
    public void End_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("abc$d")]
    [TestCase("abc$*")]
    [TestCase("abc$+")]
    [TestCase("abc$?")]
    [TestCase("abc${1}")]
    public void End_Throw(string pattern)
        => RegexTestThrow(pattern);

    [TestCase("a|b", "a", "a")]
    [TestCase("a|b", "b", "b")]
    [TestCase("a|", "a", "a")]
    [TestCase("(a|b)*aba", "abbaabac", "abbaaba")]
    [TestCase("(a|b)+aba", "abbaabac", "abbaaba")]
    [TestCase("(a|b)*", "", "")]
    public void Alternation(string pattern, string input, string expectedMatch)
        => RegexTest(pattern, input, expectedMatch);

    [TestCase("a{3}", "aaaaa", "aaa")]
    [TestCase("a{3,}", "aaaaa", "aaaaa")]
    [TestCase("a{,3}", "aaaaa", "aaa")]
    [TestCase("a{,3}", "aa", "aa")]
    [TestCase("ab{,3}", "ab", "ab")]
    [TestCase("a{3,5}", "aaa", "aaa")]
    [TestCase("a{3,5}", "aaaaaa", "aaaaa")]
    public void Range(string pattern, string input, string expectedMatch)
       => RegexTest(pattern, input, expectedMatch);

    [TestCase("a{3,5}", "aa")]
    public void Range_Fail(string pattern, string input)
       => RegexTestFail(pattern, input);

    [TestCase("a{5,3}")]
    [TestCase("a{0,0}")]
    [TestCase("a*{1}")]
    [TestCase("a{1}*")]
    [TestCase("a*{,1}")]
    [TestCase("a{3")]
    [TestCase("a{3,")]
    [TestCase("a{3,5")]
    [TestCase("{3}")]
    [TestCase("a{a}")]
    public void Range_Throw(string pattern)
         => RegexTestThrow(pattern);

    [TestCase("[a-c]*", "abcd", "abc")]
    [TestCase("[^a-c]*", "defabc", "def")]
    [TestCase("[abc]*", "abcd", "abc")]
    [TestCase("[^abc]*", "defabc", "def")]
    [TestCase("[a-cA-C]*", "abcABCd", "abcABC")]
    [TestCase("[a-cdA-CD]*", "abcdABCDe", "abcdABCD")]
    [TestCase("d[a-c]*f", "dabcf", "dabcf")]
    [TestCase(@"[\^]*", "^", "^")]
    [TestCase(@"[\]]*", "]", "]")]
    [TestCase(@"[\-]*", "-", "-")]
    [TestCase(@"[\\]*", "\\", "\\")]
    [TestCase(@"[1-9]+", "1234567890", "123456789")]
    [TestCase(@"[0-9]+", "1234567890", "1234567890")]
    [TestCase(@"[a-z]+", "abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
    [TestCase(@"[A-Z]+", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void CharacterClass(string pattern, string input, string expectedMatch)
       => RegexTest(pattern, input, expectedMatch);

    [TestCase("[a-c]", "")]
    [TestCase("[^a-c]", "")]
    [TestCase("[a-c]", "\n")]
    public void CharacterClass_Fail(string pattern, string input)
       => RegexTestFail(pattern, input);

    [Test]
    public void CharacterClass_NonPrinting()
    {
        // I put these here, instead of in the test above, because the test runner has a
        // real problem with these non-printing characters in the test name
        RegexTest("[\x01-\x1F]+", "\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10", "\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10");
        RegexTest("[\x01-\x1F]+", "\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F", "\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F");
    }

    [Test]
    public void CharacterClass_NonPrinting_Fail()
    {
        // I put these here, instead of in the test above, because the test runner has a
        // real problem with these non-printing characters in the test name
        RegexTestFail("[\x01-\x10]+", "\x1F");
    }

    [TestCase("[c-a]")]
    [TestCase("[\\")]
    [TestCase("[a")]
    [TestCase("[a\\")]
    [TestCase("[a-")]
    [TestCase("[a-\\")]
    [TestCase("[]")]
    public void CharacterClass_Throw(string pattern)
       => RegexTestThrow(pattern);

    [TestCase("a*?")]
    [TestCase("a+*")]
    [TestCase("a*+")]
    [TestCase("a?*")]
    [TestCase("?")]
    [TestCase("*")]
    [TestCase("+")]
    public void Quantifier_Errors(string pattern)
        => RegexTestThrow(pattern);

    [Test]
    public void Captures_Simple()
    {
        var target = Regex("(((...)))");
        var result = target.Parse("abc");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches.GetCapture(1, 0).Should().Be("abc");
        matches.GetCapture(2, 0).Should().Be("abc");
        matches.GetCapture(3, 0).Should().Be("abc");
    }

    [TestCase("(.)(.)(.)", "abc", "a", "b", "c")]
    [TestCase("(((..).).)", "abcd", "abcd", "abc", "ab")]
    [TestCase("(.(.(..)))", "abcd", "abcd", "bcd", "cd")]
    [TestCase("(.(.(..).).)", "abcdef", "abcdef", "bcde", "cd")]
    public void Capture3Groups1Capture(string pattern, string input, string group1, string group2, string group3)
    {
        var target = Regex(pattern);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches.GetCapture(0, 0).Should().Be(input);
        matches.GetCapture(1, 0).Should().Be(group1);
        matches.GetCapture(2, 0).Should().Be(group2);
        matches.GetCapture(3, 0).Should().Be(group3);
    }

    [TestCase("(..)+", "abcdef", "ab", "cd", "ef")]
    [TestCase("(..)*", "abcdef", "ab", "cd", "ef")]
    [TestCase("(..){3}", "abcdef", "ab", "cd", "ef")]
    public void Capture1Group3Captures(string pattern, string input, string capture1, string capture2, string capture3)
    {
        var target = Regex(pattern);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches.GetCapture(0, 0).Should().Be(input);
        matches.GetCapture(1, 0).Should().Be(capture1);
        matches.GetCapture(1, 1).Should().Be(capture2);
        matches.GetCapture(1, 2).Should().Be(capture3);
    }

    [Test]
    public void GroupMultipleBacktrack()
    {
        // The first one-or-more group will match "a" then "d". Then it will need to backtrack
        // to allow the second atom to match "d". In doing so, the capture of "d" by the group
        // in the first pass should be discarded, and only "a" should be captured.
        var target = Regex("(.)+d");
        var result = target.Parse("ad");
        result.Success.Should().BeTrue();
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches.GetCapture(0, 0).Should().Be("ad");
        matches.Count.Should().Be(2);
        matches.GetGroup(1).Count.Should().Be(1);
        matches.GetCapture(1, 0).Should().Be("a");
    }

    [TestCase("(?:.)", "a")]
    [TestCase("(?:..)", "ab")]
    [TestCase("(?:..)*", "abcd")]
    [TestCase("(?:..)+", "abcd")]
    [TestCase("(?:..){1,}", "abcd")]
    [TestCase("(?:..){,4}", "abcdefgh")]
    public void NonCapturingCloister_NoCaptures(string pattern, string input)
    {
        var target = Regex(pattern);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches.Count.Should().Be(1);
        matches[0][0].Should().Be(input);
    }

    [Test]
    public void NonCapturingCloister_FailEndOfInput()
    {
        var target = Regex("...(?:.)");
        var result = target.Parse("abc");
        result.Success.Should().BeFalse();
    }

    [TestCase("a(?=b)", "ab", "a")]
    [TestCase("a(?=.)", "ac", "a")]
    [TestCase("a(?=.)b", "ab", "ab")]
    [TestCase("(?=a)", "a", "")]
    public void PositiveLookahead(string pattern, string input, string expected)
        => RegexTest(pattern, input, expected);

    [TestCase("a(?=b)", "a")]
    [TestCase("a(?=b)", "ac")]
    public void PositiveLookahead_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("a(?=)")]
    [TestCase("a(?=b)*")]
    [TestCase("a(?=b)?")]
    [TestCase("a(?=b){0,1}")]
    public void PositiveLookahead_ParseFail(string pattern)
        => RegexTestThrow(pattern);

    [TestCase("a(?!b)", "ac", "a")]
    [TestCase("a(?!.)", "a", "a")]
    [TestCase("(?!b)", "a", "")]
    public void NegativeLookahead(string pattern, string input, string expected)
        => RegexTest(pattern, input, expected);

    [TestCase("a(?!b)", "ab")]
    public void NegativeLookahead_Fail(string pattern, string input)
        => RegexTestFail(pattern, input);

    [TestCase("a(?!)")]
    [TestCase("a(?!b)*")]
    [TestCase("a(?!b)?")]
    [TestCase("a(?!b){0,1}")]
    public void NegativeLookahead_ParseFail(string pattern)
        => RegexTestThrow(pattern);

    [TestCase("(..)\\1", "abab", "abab", "ab")]
    [TestCase("(..)(\\1|ad)", "abad", "abad", "ab")] // partial backref match should rewind successfully
    [TestCase("(?:..)(..)\\1", "abcdcd", "abcdcd", "cd")] // non-capturing cloister isn't used
    [TestCase("(..)\\1*", "ababab", "ababab", "ab")]
    [TestCase("(..)\\1+", "ababab", "ababab", "ab")]
    [TestCase("(..)\\1{1}", "ababab", "abab", "ab")]
    [TestCase("(..)\\1{1,}", "ababab", "ababab", "ab")]
    [TestCase("(..)\\1{,2}", "ababab", "ababab", "ab")]
    public void CaptureBackreference(string pattern, string input, string group0, string group1)
    {
        var target = Regex(pattern);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(group0);
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches[0][0].Should().Be(group0);
        matches[1][0].Should().Be(group1);
    }

    [TestCase("(..)(..)\\2\\1", "abcdcdab", "abcdcdab", "ab", "cd")]
    public void CaptureBackreference(string pattern, string input, string group0, string group1, string group2)
    {
        var target = Regex(pattern);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(group0);
        var matchOption = result.Data.OfType<RegexMatch>();
        matchOption.Success.Should().BeTrue();
        var matches = matchOption.Value;
        matches[0][0].Should().Be(group0);
        matches[1][0].Should().Be(group1);
        matches[2][0].Should().Be(group2);
    }

    [TestCase("a(?{b})*c", "abbbbbc", "abbbbbc")]
    [TestCase("a(?{b})?c", "ac", "ac")]
    [TestCase("a((?{b}))c\\1", "abcb", "abcb")]
    public void ParserRecurse(string pattern, string input, string expected)
    {
        var a = MatchChar('a').Named("a");
        var b = MatchChar('b').Named("b");
        var c = MatchChar('c');
        var target = Regex(pattern, a, b, c);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("a(?{p})c", "abc", "abc")]
    public void ParserRecurse_Data(string pattern, string input, string expected)
    {
        // Test to make sure the IParseState<char>.Data field works in an inner parser
        var inner = DataContext(MatchChar('b')).Named("p");
        var target = Regex(pattern, inner);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("a(?{p})(?{p})c", "abbc", "abbc")]
    public void ParserRecurse_Cache(string pattern, string input, string expected)
    {
        // Test to make sure the IParseState<char>.Cache field works in an inner parser
        var inner = Deferred(() => MatchChar('b')).Named("p");
        var target = Regex(pattern, inner);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Test]
    public void Match_Test()
    {
        var target = Regex("abc");
        var input = FromString("abcdef");
        var result = target.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(3);
    }

    [Test]
    public void Match_Fail()
    {
        var target = Regex("abX");
        var input = FromString("abcdef");
        var result = target.Match(input);
        result.Should().BeFalse();
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = Regex("(a|b)c*d").Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := /(a|b)c*d/");
    }
}
