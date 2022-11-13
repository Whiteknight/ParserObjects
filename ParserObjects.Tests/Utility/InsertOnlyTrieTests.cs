using System.Linq;
using ParserObjects.Internal;
using ParserObjects.Internal.Utility;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Utility
{
    public class InsertOnlyTrieTests
    {
        [Test]
        public void Char_Int_AddGet()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            target.Get("abc").Value.Should().Be(1);
            target.Get("abd").Value.Should().Be(2);
            target.Get("aef").Value.Should().Be(3);
            target.Get("aeg").Value.Should().Be(4);
            target.Get("hij").Value.Should().Be(5);
        }

        [Test]
        public void Char_Int_AddGetBacktrack()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abcde", 2);

            // looks for "abcd", has a node but no value. Then backtracks to "abc" and finds the value
            var input = FromString("abcd");

            target.Get(input).Value.Should().Be(1);
        }

        [Test]
        public void Char_Int_DoesntExist()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            target.Get("XYZ").Success.Should().BeFalse();
        }

        [Test]
        public void Char_Int_AddGetPrefixes()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("a", 1);
            target.Add("ab", 2);
            target.Add("abc", 3);
            target.Add("abcd", 4);

            target.Get("a").Value.Should().Be(1);
            target.Get("ab").Value.Should().Be(2);
            target.Get("abc").Value.Should().Be(3);
            target.Get("abcd").Value.Should().Be(4);
        }

        [Test]
        public void Char_Int_DuplicateAddGet()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);

            target.Get("abc").Value.Should().Be(1);
        }

        [Test]
        public void Char_Int_AddConflict()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            Action act = () => target.Add("abc", 2);
            act.Should().Throw<TrieInsertException>();
        }

        [Test]
        public void Char_Int_AddGetSequence()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            target.Get(FromString("abc")).Value.Should().Be(1);
            target.Get(FromString("abd")).Value.Should().Be(2);
            target.Get(FromString("aef")).Value.Should().Be(3);
            target.Get(FromString("aeg")).Value.Should().Be(4);
            target.Get(FromString("hij")).Value.Should().Be(5);

            target.Get(FromString("abX")).Success.Should().BeFalse();
            target.Get(FromString("aXc")).Success.Should().BeFalse();
            target.Get(FromString("Xbc")).Success.Should().BeFalse();
        }

        [Test]
        public void Char_Int_GetAllPatterns()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            var result = target.GetAllPatterns().ToList();
            result.Count.Should().Be(5);
            result[0].Should().BeEquivalentTo(new[] { 'a', 'b', 'c' });
            result[1].Should().BeEquivalentTo(new[] { 'a', 'b', 'd' });
            result[2].Should().BeEquivalentTo(new[] { 'a', 'e', 'f' });
            result[3].Should().BeEquivalentTo(new[] { 'a', 'e', 'g' });
            result[4].Should().BeEquivalentTo(new[] { 'h', 'i', 'j' });
        }

        [Test]
        public void Char_Int_GetAllPatterns_Duplicate()
        {
            var target = new InsertOnlyTrie<char, int>();
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);

            var result = target.GetAllPatterns().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeEquivalentTo(new[] { 'a', 'b', 'c' });
        }
    }
}
