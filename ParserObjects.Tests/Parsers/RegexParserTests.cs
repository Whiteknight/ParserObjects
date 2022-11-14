using System.IO;
using System.Text;
using ParserObjects.Regexes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class RegexParserTests
    {
        [Test]
        public void Regex_MatchLiteral_CanReparse()
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
        public void Regex_Combines_Nicely()
        {
            var parser = Rule(
                Match('['),
                Regex("abc*"),
                Match(']'),
                (open, match, close) => match
            );
            var result = parser.Parse("[abcccccc]");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcccccc");
        }

        private void RegexTest(string pattern, string input, string expectedMatch)
        {
            var parser = Regex(pattern);

            // First test with a string sequence
            var stringSequence = FromString(input);
            RegexTest(expectedMatch, parser, stringSequence);

            // Second test with a stream sequence, just to show that they are equivalent
            var bytes = Encoding.UTF8.GetBytes(input);
            using var memoryStream = new MemoryStream(bytes);
            var streamSequence = FromStream(memoryStream, Encoding.UTF8);
            RegexTest(expectedMatch, parser, streamSequence);
        }

        private static void RegexTest(string expectedMatch, IParser<char, string> parser, ISequence<char> sequence)
        {
            var result = parser.Parse(sequence);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(expectedMatch);
            result.Consumed.Should().Be(expectedMatch.Length);
            sequence.Consumed.Should().Be(expectedMatch.Length);
        }

        private void RegexTestThrow(string pattern)
        {
            Action act = () => Regex(pattern);
            act.Should().Throw<RegexException>();
        }

        private void RegexTestFail(string pattern, string input)
        {
            var sequence = FromString(input);
            var parser = Regex(pattern);
            var result = parser.Parse(sequence);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            sequence.Consumed.Should().Be(0);
        }

        [TestCase("abc", "abcd", "abc")]
        public void Regex_Match(string pattern, string input, string expectedMatch)
            => RegexTest(pattern, input, expectedMatch);

        [TestCase("a", "bad")]
        public void Regex_Match_Fail(string pattern, string input)
            => RegexTestFail(pattern, input);

        [TestCase("ab?c", "abcd", "abc")]
        [TestCase("ab?c", "acd", "ac")]
        [TestCase("ab?", "a", "a")]
        [TestCase("ab?", "abbb", "ab")]
        [TestCase("a?b", "b", "b")]
        [TestCase("a?b", "ab", "ab")]
        public void Regex_ZeroOrOne(string pattern, string input, string expectedMatch)
             => RegexTest(pattern, input, expectedMatch);

        [TestCase("ab*c", "abbbbcd", "abbbbc")]
        [TestCase("ab*c", "acd", "ac")]
        [TestCase("ab*b", "abcd", "ab")]
        [TestCase("ab*b", "abbbbcd", "abbbb")]
        public void Regex_ZeroOrMore(string pattern, string input, string expectedMatch)
             => RegexTest(pattern, input, expectedMatch);

        [TestCase("ab+c", "abbbbcd", "abbbbc")]
        [TestCase("ab+c", "abcd", "abc")]
        public void Regex_OneOrMore(string pattern, string input, string expectedMatch)
             => RegexTest(pattern, input, expectedMatch);

        [TestCase("ab+c", "acde")]
        public void Regex_OneOrMore_Fail(string pattern, string input)
             => RegexTestFail(pattern, input);

        [TestCase("a(bc)?bc", "abcbcf", "abcbc")]
        [TestCase("a(bc)?bc", "abcdef", "abc")]
        [TestCase("a(bc)*bc", "abcbcbcdef", "abcbcbc")]
        public void Regex_Groups(string pattern, string input, string expectedMatch)
             => RegexTest(pattern, input, expectedMatch);

        [TestCase("a$(b)")]
        public void Regex_Groups_Throw(string pattern)
             => RegexTestThrow(pattern);

        [TestCase("\\d", "1a", "1")]
        [TestCase("\\D", "a1", "a")]
        [TestCase("\\w", "a ", "a")]
        [TestCase("\\W", " a", " ")]
        [TestCase("\\s", " a", " ")]
        [TestCase("\\S", "a ", "a")]
        [TestCase("(\\w+\\s)*\\w+", "this is a test.", "this is a test")]
        public void Regex_SpecialClasses(string pattern, string input, string expectedMatch)
             => RegexTest(pattern, input, expectedMatch);

        [TestCase("\\d", "a1")]
        [TestCase("\\D", "1a")]
        [TestCase("\\w", " a")]
        [TestCase("\\W", "a ")]
        [TestCase("\\s", "a ")]
        [TestCase("\\S", " a")]
        public void Regex_SpecialClasses_Fail(string pattern, string input)
            => RegexTestFail(pattern, input);

        [TestCase(".*", "abcd+=.", "abcd+=.")]
        [TestCase("\\.", ".", ".")]
        [TestCase(".*\\..*", "command.com", "command.com")]
        public void Regex_Wildcard(string pattern, string input, string expectedMatch)
            => RegexTest(pattern, input, expectedMatch);

        [TestCase("\\.", "a")]
        public void Regex_Wildcard_Fail(string pattern, string input)
            => RegexTestFail(pattern, input);

        [TestCase("abc$", "abc", "abc")]
        [TestCase("abc\\$", "abc$def", "abc$")]
        [TestCase("abc|abd$|abe", "abd", "abd")]
        public void Regex_End(string pattern, string input, string expectedMatch)
            => RegexTest(pattern, input, expectedMatch);

        [TestCase("abc$", "abcd")]
        [TestCase("abc|abd$|abe", "abde")]
        public void Regex_End_Fail(string pattern, string input)
            => RegexTestFail(pattern, input);

        [TestCase("abc$d")]
        [TestCase("abc$*")]
        [TestCase("abc$+")]
        [TestCase("abc$?")]
        [TestCase("abc${1}")]
        public void Regex_End_Throw(string pattern)
            => RegexTestThrow(pattern);

        [TestCase("a|b", "a", "a")]
        [TestCase("a|b", "b", "b")]
        [TestCase("a|", "a", "a")]
        [TestCase("(a|b)*aba", "abbaabac", "abbaaba")]
        public void Regex_Alternation(string pattern, string input, string expectedMatch)
            => RegexTest(pattern, input, expectedMatch);

        [TestCase("a{3}", "aaaaa", "aaa")]
        [TestCase("a{3,}", "aaaaa", "aaaaa")]
        [TestCase("a{,3}", "aaaaa", "aaa")]
        [TestCase("a{,3}", "aa", "aa")]
        [TestCase("ab{,3}", "ab", "ab")]
        [TestCase("a{3,5}", "aaa", "aaa")]
        [TestCase("a{3,5}", "aaaaaa", "aaaaa")]
        public void Regex_Range(string pattern, string input, string expectedMatch)
           => RegexTest(pattern, input, expectedMatch);

        [TestCase("a{3,5}", "aa")]
        public void Regex_Range_Fail(string pattern, string input)
           => RegexTestFail(pattern, input);

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
        public void Regex_CharacterClass(string pattern, string input, string expectedMatch)
           => RegexTest(pattern, input, expectedMatch);

        [TestCase("[c-a]")]
        [TestCase("[a-]")]
        [TestCase("[a")]
        [TestCase("[a-")]
        [TestCase("[]")]
        public void Regex_CharacterClass_Throw(string pattern)
           => RegexTestThrow(pattern);

        [TestCase("a*?")]
        [TestCase("a+*")]
        [TestCase("a*+")]
        [TestCase("a?*")]
        [TestCase("?")]
        [TestCase("*")]
        [TestCase("+")]
        public void Regex_Quantifier_Errors(string pattern)
            => RegexTestThrow(pattern);

        [TestCase("a{5,3}")]
        [TestCase("a{0,0}")]
        [TestCase("a*{1}")]
        [TestCase("a{1}*")]
        [TestCase("a*{,1}")]
        [TestCase("a{1")]
        public void Regex_Range_Errors(string pattern)
             => RegexTestThrow(pattern);

        [Test]
        public void Parse_MaxItemsLimit()
        {
            // The pattern should be able to match all 6 items in the string, but we limit the
            // buffer to only 4 items so it cannot go further.
            var target = Regex("(..)+", 4);
            var result = target.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(4);
            result.Value.Should().Be("abcd");
        }

        [Test]
        public void Parse_Captures_Simple()
        {
            var target = Regex("(((...)))");
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
            var matchOption = result.TryGetData<RegexMatch>();
            matchOption.Success.Should().BeTrue();
            var matches = matchOption.Value;
            matches.Groups[1][0].Should().Be("abc");
            matches.Groups[2][0].Should().Be("abc");
            matches.Groups[3][0].Should().Be("abc");
        }

        [TestCase("(.)(.)(.)", "abc", "a", "b", "c")]
        [TestCase("(((..).).)", "abcd", "abcd", "abc", "ab")]
        [TestCase("(.(.(..)))", "abcd", "abcd", "bcd", "cd")]
        [TestCase("(.(.(..).).)", "abcdef", "abcdef", "bcde", "cd")]
        public void Parse_3Groups1Capture(string pattern, string input, string group1, string group2, string group3)
        {
            var target = Regex(pattern);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            var matchOption = result.TryGetData<RegexMatch>();
            matchOption.Success.Should().BeTrue();
            var matches = matchOption.Value;
            matches.Groups[0][0].Should().Be(input);
            matches.Groups[1][0].Should().Be(group1);
            matches.Groups[2][0].Should().Be(group2);
            matches.Groups[3][0].Should().Be(group3);
        }

        [TestCase("(..)+", "abcdef", "ab", "cd", "ef")]
        [TestCase("(..)*", "abcdef", "ab", "cd", "ef")]
        [TestCase("(..){3}", "abcdef", "ab", "cd", "ef")]
        public void Parse_1Group3Captures(string pattern, string input, string capture1, string capture2, string capture3)
        {
            var target = Regex(pattern);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            var matchOption = result.TryGetData<RegexMatch>();
            matchOption.Success.Should().BeTrue();
            var matches = matchOption.Value;
            matches.Groups[0][0].Should().Be(input);
            matches.Groups[1][0].Should().Be(capture1);
            matches.Groups[1][1].Should().Be(capture2);
            matches.Groups[1][2].Should().Be(capture3);
        }

        [Test]
        public void Parse_GroupMultipleBacktrack()
        {
            // The first one-or-more group will match "a" then "d". Then it will need to backtrack
            // to allow the second atom to match "d". In doing so, the capture of "d" by the group
            // in the first pass should be discarded, and only "a" should be captured.
            var target = Regex("(.)+d");
            var result = target.Parse("ad");
            result.Success.Should().BeTrue();
            var matchOption = result.TryGetData<RegexMatch>();
            matchOption.Success.Should().BeTrue();
            var matches = matchOption.Value;
            matches.Groups[0][0].Should().Be("ad");
            matches.Groups.Count.Should().Be(2);
            matches.Groups[1].Count.Should().Be(1);
            matches.Groups[1][0].Should().Be("a");
        }

        [TestCase("(?:.)", "a")]
        [TestCase("(?:..)", "ab")]
        public void Parse_NonCapturingCloister_NoCaptures(string pattern, string input)
        {
            var target = Regex(pattern);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            var matchOption = result.TryGetData<RegexMatch>();
            matchOption.Success.Should().BeTrue();
            var matches = matchOption.Value;
            matches.Groups.Count.Should().Be(1);
            matches.Groups[0][0].Should().Be(input);
        }

        [TestCase("(..)\\1", "abab")]
        public void Parse_CaptureBackreference(string pattern, string input)
        {
            var target = Regex(pattern);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(input);
        }
    }
}
