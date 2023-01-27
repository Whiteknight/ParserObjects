using System.Linq;
using ParserObjects.Internal.Tries;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests
{
    public class TrieTests
    {
        [Test]
        public void Char_Int_AddGet()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            var readable = target.Freeze();
            readable.Get("abc").Value.Should().Be(1);
            readable.Get("abd").Value.Should().Be(2);
            readable.Get("aef").Value.Should().Be(3);
            readable.Get("aeg").Value.Should().Be(4);
            readable.Get("hij").Value.Should().Be(5);
        }

        [Test]
        public void Char_Int_AddGetBacktrack()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abcde", 2);

            // looks for "abcd", has a node but no value. Then backtracks to "abc" and finds the value
            var input = FromString("abcd");

            var readable = target.Freeze();
            readable.Get(input).Value.Should().Be(1);
        }

        [Test]
        public void Char_Int_DoesntExist()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            var readable = target.Freeze();
            readable.Get("XYZ").Success.Should().BeFalse();
        }

        [Test]
        public void Char_Int_AddGetPrefixes()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("a", 1);
            target.Add("ab", 2);
            target.Add("abc", 3);
            target.Add("abcd", 4);

            var readable = target.Freeze();
            readable.Get("a").Value.Should().Be(1);
            readable.Get("ab").Value.Should().Be(2);
            readable.Get("abc").Value.Should().Be(3);
            readable.Get("abcd").Value.Should().Be(4);
        }

        [Test]
        public void Char_Int_DuplicateAddGet()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);

            var readable = target.Freeze();
            readable.Get("abc").Value.Should().Be(1);
        }

        [Test]
        public void Char_Int_AddConflict()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            Action act = () => target.Add("abc", 2);
            act.Should().Throw<TrieInsertException>();
        }

        [Test]
        public void Char_Int_AddGetSequence()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            var readable = target.Freeze();
            readable.Get(FromString("abc")).Value.Should().Be(1);
            readable.Get(FromString("abd")).Value.Should().Be(2);
            readable.Get(FromString("aef")).Value.Should().Be(3);
            readable.Get(FromString("aeg")).Value.Should().Be(4);
            readable.Get(FromString("hij")).Value.Should().Be(5);

            readable.Get(FromString("abX")).Success.Should().BeFalse();
            readable.Get(FromString("aXc")).Success.Should().BeFalse();
            readable.Get(FromString("Xbc")).Success.Should().BeFalse();
        }

        [Test]
        public void Char_Int_GetAllPatterns()
        {
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abd", 2);
            target.Add("aef", 3);
            target.Add("aeg", 4);
            target.Add("hij", 5);

            var readable = target.Freeze();
            var result = readable.GetAllPatterns().ToList();
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
            var target = InsertableTrie<char, int>.Create();
            target.Add("abc", 1);
            target.Add("abc", 1);
            target.Add("abc", 1);

            var readable = target.Freeze();
            var result = readable.GetAllPatterns().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeEquivalentTo(new[] { 'a', 'b', 'c' });
        }
    }
}
