﻿using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class TrieTests
{
    public class MultiAction
    {
        [Test]
        public void Parse()
        {
            var target = TrieMulti<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add("===", "===")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

            var input = FromString("===>=<=><<==");

            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be("=");
            result.Results[0].Consumed.Should().Be(1);
            result.Results[1].Value.Should().Be("==");
            result.Results[1].Consumed.Should().Be(2);
            result.Results[2].Value.Should().Be("===");
            result.Results[2].Consumed.Should().Be(3);
        }

        [Test]
        public void Parse_Continue()
        {
            static void Populate(InsertableTrie<char, string> trie)
            {
                trie.Add("=", "=");
                trie.Add("==", "==");
                trie.Add("===", "===");
                trie.Add(">=", ">=");
                trie.Add("<=", "<=");
                trie.Add("<", "<");
                trie.Add(">", ">");
            }

            var multiTrieParser = TrieMulti<string>(Populate);
            var singleTrieParser = Trie<string>(Populate);

            var target = multiTrieParser.ContinueWith(left => Rule(
                left,
                singleTrieParser,
                (l, r) => $"{l} {r}"
            ));
            var input = FromString("===>=<=><<==");

            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be("= ==");
            result.Results[1].Value.Should().Be("== =");
            result.Results[2].Value.Should().Be("=== >=");
        }
    }

    public class MultiInstance
    {
        [Test]
        public void Parse_EmptyTrie()
        {
            var trie = InsertableTrie<char, int>.Create();
            var target = TrieMulti(trie);

            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
        }
    }

    public class SingleAction
    {
        [Test]
        public void Parse_IsAtEnd()
        {
            var target = Trie<string>(trie => trie
                .Add("\0\0\0", "ok")
            );

            var input = FromString("\0");

            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Operators()
        {
            var target = Trie<string>(trie =>
            {
                trie.Add("=", "=");
                trie.Add("==", "==");
                trie.Add(">=", ">=");
                trie.Add("<=", "<=");
                trie.Add("<", "<");
                trie.Add(">", ">");
            });

            var input = FromString("===>=<=><<==");

            target.Parse(input).Value.Should().Be("==");
            target.Parse(input).Value.Should().Be("=");
            target.Parse(input).Value.Should().Be(">=");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be(">");
            target.Parse(input).Value.Should().Be("<");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be("=");
        }

        [Test]
        public void Parse_Operators_Fail()
        {
            var target = Trie<string>(trie =>
            {
                trie.Add("=", "=");
                trie.Add("==", "==");
                trie.Add(">=", ">=");
                trie.Add("<=", "<=");
                trie.Add("<", "<");
                trie.Add(">", ">");
            });

            var input = FromString("X===>=<=><<==");

            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Consumed()
        {
            var target = Trie<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

            var input = FromString("===X");

            var result = target.Parse(input);
            result.Value.Should().Be("==");
            result.Consumed.Should().Be(2);

            result = target.Parse(input);
            result.Value.Should().Be("=");
            result.Consumed.Should().Be(1);

            result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [TestCase("=")]
        [TestCase("==")]
        [TestCase(">=")]
        [TestCase("<=")]
        [TestCase("<")]
        [TestCase(">")]
        public void Match_Operators(string oper)
        {
            var target = Trie<string>(trie =>
            {
                trie.Add("=", "=");
                trie.Add("==", "==");
                trie.Add(">=", ">=");
                trie.Add("<=", "<=");
                trie.Add("<", "<");
                trie.Add(">", ">");
            });

            var input = FromString(oper);

            target.Match(input).Should().BeTrue();
            input.Consumed.Should().Be(oper.Length);
        }

        [Test]
        public void Match_Operators_Fail()
        {
            var target = Trie<string>(trie =>
            {
                trie.Add("=", "=");
                trie.Add("==", "==");
                trie.Add(">=", ">=");
                trie.Add("<=", "<=");
                trie.Add("<", "<");
                trie.Add(">", ">");
            });

            var input = FromString("X");
            target.Match(input).Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Trie<char>(_ => { });
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Trie<string>(trie => trie
                    .Add("abc")
                    .Add("abd")
                    .Add("xyz")
                )
                .Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ('a' 'b' 'c') | ('a' 'b' 'd') | ('x' 'y' 'z')");
        }

        [Test]
        public void ToBnf_Single()
        {
            var parser = Trie<string>(trie => trie
                    .Add("abc")
                )
                .Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ('a' 'b' 'c')");
        }

        [Test]
        public void ToBnf_Duplicate()
        {
            var parser = Trie<string>(trie => trie
                    .Add("abc")
                    .Add("abc")
                    .Add("abc")
                )
                .Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ('a' 'b' 'c')");
        }

        [Test]
        public void ToBnf_Empty()
        {
            var parser = Trie<string>(trie => { }).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := FAIL");
        }
    }

    public class SingleInstance
    {
        [Test]
        public void Parse_StringInt()
        {
            var trie = InsertableTrie<char, int>.Create();
            trie.Add("abc", 1);
            trie.Add("abd", 2);
            trie.Add("aef", 3);
            trie.Add("aeg", 4);
            trie.Add("hij", 5);
            var target = Trie(trie);

            var input = FromString("abcabdaefaeghij");
            target.Parse(input).Value.Should().Be(1);
            target.Parse(input).Value.Should().Be(2);
            target.Parse(input).Value.Should().Be(3);
            target.Parse(input).Value.Should().Be(4);
            target.Parse(input).Value.Should().Be(5);
        }

        [Test]
        public void Parse_EmptyTrie()
        {
            var trie = InsertableTrie<char, int>.Create();
            var target = Trie(trie);

            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_StringInt_AddGetBacktrack()
        {
            var trie = InsertableTrie<char, int>.Create();
            trie.Add("abc", 1);
            trie.Add("abcde", 2);
            var target = Trie(trie);

            // looks for "abcd", has a node but no value. Then backtracks to "abc" and finds the value
            var input = FromString("abcd");

            target.Parse(input).Value.Should().Be(1);
        }

        [TestCase("a", 1)]
        [TestCase("ab", 2)]
        [TestCase("abc", 3)]
        [TestCase("abcd", 4)]
        public void Parse_StringInt_AddGetPrefixes(string input, int result)
        {
            var trie = InsertableTrie<char, int>.Create();
            trie.Add("a", 1);
            trie.Add("ab", 2);
            trie.Add("abc", 3);
            trie.Add("abcd", 4);

            var target = Trie(trie);
            target.Parse(input).Value.Should().Be(result);
        }

        [Test]
        public void Parse_StringInt_Duplicate_WrongValue()
        {
            var trie = InsertableTrie<char, int>.Create();
            trie.Add("abc", 1);

            Action act = () => trie.Add("abc", 2);
            act.Should().Throw<TrieInsertException>();
        }

        [Test]
        public void Parse_StringInt_Duplicate_SameValue()
        {
            var trie = InsertableTrie<char, int>.Create();
            trie.Add("abc", 1);
            trie.Add("abc", 1);

            var target = Trie(trie);
            target.Parse("abc").Value.Should().Be(1);
        }
    }
}
