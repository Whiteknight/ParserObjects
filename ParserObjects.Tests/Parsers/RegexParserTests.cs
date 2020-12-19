using System;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Regexes;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class RegexParserTests
    {
        [Test]
        public void Regex_MatchLiteral()
        {
            var parser = Regex("abc");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

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
        public void Regex_MatchZeroOrOne_Zero()
        {
            var parser = Regex("ab?c");
            var result = parser.Parse("acdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ac");
        }

        [Test]
        public void Regex_MatchZeroOrOne_ZeroEnd()
        {
            var parser = Regex("ab?");
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_MatchZeroOrOne_One()
        {
            var parser = Regex("ab?c");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_MatchZeroOrMore_NonBacktracking_Four()
        {
            var parser = Regex("ab*c");
            var result = parser.Parse("abbbbcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abbbbc");
        }

        [Test]
        public void Regex_MatchZeroOrMore_NonBacktracking_Zero()
        {
            var parser = Regex("ab*c");
            var result = parser.Parse("acdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ac");
        }

        [Test]
        public void Regex_MatchZeroOrMore_Backtracking_Three()
        {
            var parser = Regex("ab*b");
            var result = parser.Parse("abbbbcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abbbb");
        }

        [Test]
        public void Regex_MatchZeroOrMore_Backtracking_Zero()
        {
            var parser = Regex("ab*b");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ab");
        }

        [Test]
        public void Regex_MatchOneOrMore_NonBacktracking_Four()
        {
            var parser = Regex("ab+c");
            var result = parser.Parse("abbbbcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abbbbc");
        }

        [Test]
        public void Regex_MatchOneOrMore_NonBacktracking_One()
        {
            var parser = Regex("ab+c");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_MatchOneOrMore_NonBacktracking_Zero()
        {
            var parser = Regex("ab+c");
            var result = parser.Parse("acdef");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_GroupZeroOrOne_NonBacktracking_One()
        {
            var parser = Regex("a(bc)?bc");
            var result = parser.Parse("abcbcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcbc");
        }

        [Test]
        public void Regex_GroupZeroOrOne_Backtracking_Zero()
        {
            var parser = Regex("a(bc)?bc");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_GroupZeroOrMore_Backtracking_One()
        {
            var parser = Regex("a(bc)*bc");
            var result = parser.Parse("abcbcbcdef");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcbcbc");
        }

        [Test]
        public void Regex_Digits_Matches()
        {
            var parser = Regex("\\d");
            var result = parser.Parse("1a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void Regex_Digits_NotMatches()
        {
            var parser = Regex("\\d");
            var result = parser.Parse("a1");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_NonDigits_Matches()
        {
            var parser = Regex("\\D");
            var result = parser.Parse("a1");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_NonDigits_NotMatches()
        {
            var parser = Regex("\\D");
            var result = parser.Parse("1a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_WordChar_Matches()
        {
            var parser = Regex("\\w");
            var result = parser.Parse("a ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_WordChar_NotMatches()
        {
            var parser = Regex("\\w");
            var result = parser.Parse(" a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_NonWordChar_Matches()
        {
            var parser = Regex("\\W");
            var result = parser.Parse(" a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(" ");
        }

        [Test]
        public void Regex_NonWordChar_NotMatches()
        {
            var parser = Regex("\\W");
            var result = parser.Parse("a ");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_Whitespace_Matches()
        {
            var parser = Regex("\\s");
            var result = parser.Parse(" a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(" ");
        }

        [Test]
        public void Regex_Whitespace_NotMatches()
        {
            var parser = Regex("\\s");
            var result = parser.Parse("a ");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_NonWhitespace_Matches()
        {
            var parser = Regex("\\S");
            var result = parser.Parse("a ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_NonWhitespace_NotMatches()
        {
            var parser = Regex("\\S");
            var result = parser.Parse(" a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_WildcardZeroOrMore_Matches()
        {
            var parser = Regex(".*");
            var result = parser.Parse("abcd+=.");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcd+=.");
        }

        [Test]
        public void Regex_EscapedWildcard_Matches()
        {
            var parser = Regex("\\.");
            var result = parser.Parse(".");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(".");
        }

        [Test]
        public void Regex_EscapedWildcard_NotMatches()
        {
            var parser = Regex("\\.");
            var result = parser.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_End_Matches()
        {
            var parser = Regex("abc$");
            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_End_NotMatches()
        {
            var parser = Regex("abc$");
            var result = parser.Parse("abcdef");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_End_NotAtEndOfPattern()
        {
            Action act = () => Regex("abc$d");
            act.Should().Throw<Exception>();
        }

        [Test]
        public void Regex_End_Quantified()
        {
            Action act = () => Regex("abc$*");
            act.Should().Throw<Exception>();
        }

        [Test]
        public void Regex_EscapedEnd_NotAtEndOfPattern()
        {
            var parser = Regex("abc\\$");
            var result = parser.Parse("abc$def");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc$");
        }

        [Test]
        public void Regex_Alternation_First()
        {
            var parser = Regex("a|b");
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_Alternation_Second()
        {
            var parser = Regex("a|b");
            var result = parser.Parse("b");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("b");
        }

        [Test]
        public void Regex_GroupAlternationZeroOrMore_Backtracking()
        {
            var parser = Regex("(a|b)*aba");
            var result = parser.Parse("abbaabac");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abbaaba");
        }

        [Test]
        public void Regex_RangeExactlyThree_Match()
        {
            var parser = Regex("a{3}");
            var result = parser.Parse("aaaaa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aaa");
        }

        [Test]
        public void Regex_RangeAtLeastThree_Match()
        {
            var parser = Regex("a{3,}");
            var result = parser.Parse("aaaaa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aaaaa");
        }

        [Test]
        public void Regex_RangeAtMostThree_MatchThree()
        {
            var parser = Regex("a{,3}");
            var result = parser.Parse("aaaaa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aaa");
        }

        [Test]
        public void Regex_RangeAtMostThree_MatchTwo()
        {
            var parser = Regex("a{,3}");
            var result = parser.Parse("aa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aa");
        }

        [Test]
        public void Regex_RangeAtMostThree_MatchZeroEnd()
        {
            var parser = Regex("ab{,3}");
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("a");
        }

        [Test]
        public void Regex_RangeThreeToFive_MatchThree()
        {
            var parser = Regex("a{3,5}");
            var result = parser.Parse("aaa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aaa");
        }

        [Test]
        public void Regex_RangeThreeToFive_MatchFive()
        {
            var parser = Regex("a{3,5}");
            var result = parser.Parse("aaaaaaa");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aaaaa");
        }

        [Test]
        public void Regex_RangeThreeToFive_TooFew()
        {
            var parser = Regex("a{3,5}");
            var result = parser.Parse("aa");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Regex_CharacterClassRange_ZeroOrMore()
        {
            var parser = Regex("[a-c]*");
            var result = parser.Parse("abcd");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_CharacterClassChars_ZeroOrMore()
        {
            var parser = Regex("[abc]*");
            var result = parser.Parse("abcd");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }

        [Test]
        public void Regex_CharacterClassRanges_ZeroOrMore()
        {
            var parser = Regex("[a-cA-C]*");
            var result = parser.Parse("abcABCd");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcABC");
        }

        [Test]
        public void Regex_CharacterClassMulti_ZeroOrMore()
        {
            var parser = Regex("[a-cdA-CD]*");
            var result = parser.Parse("abcdABCDe");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcdABCD");
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

        [Test]
        public void Regex_Error_QuestionAfterStar()
        {
            Action act = () => Regex("a*?");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_StarAfterPlus()
        {
            Action act = () => Regex("a+*");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_PlusAfterStar()
        {
            Action act = () => Regex("a*+");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_RangeInverted()
        {
            Action act = () => Regex("a{5,3}");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_RangeZeroMax()
        {
            Action act = () => Regex("a{0,0}");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_RangeMinimumAfterStar()
        {
            Action act = () => Regex("a*{1}");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_StarAfterRangeMinimum()
        {
            Action act = () => Regex("a{1}*");
            act.Should().Throw<RegexException>();
        }

        [Test]
        public void Regex_Error_RangeMaximumAfterStar()
        {
            Action act = () => Regex("a*{,1}");
            act.Should().Throw<RegexException>();
        }
    }
}
