﻿using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects;
using ParserObjects.Sequences;
using ParserObjects.Utility;

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
            var input = new StringCharacterSequence("abcd");

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

            target.Get(new StringCharacterSequence("abc")).Value.Should().Be(1);
            target.Get(new StringCharacterSequence("abd")).Value.Should().Be(2);
            target.Get(new StringCharacterSequence("aef")).Value.Should().Be(3);
            target.Get(new StringCharacterSequence("aeg")).Value.Should().Be(4);
            target.Get(new StringCharacterSequence("hij")).Value.Should().Be(5);

            target.Get(new StringCharacterSequence("abX")).Success.Should().BeFalse();
            target.Get(new StringCharacterSequence("aXc")).Success.Should().BeFalse();
            target.Get(new StringCharacterSequence("Xbc")).Success.Should().BeFalse();
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
            result[0].Should().BeEquivalentTo('a', 'b', 'c');
            result[1].Should().BeEquivalentTo('a', 'b', 'd');
            result[2].Should().BeEquivalentTo('a', 'e', 'f');
            result[3].Should().BeEquivalentTo('a', 'e', 'g');
            result[4].Should().BeEquivalentTo('h', 'i', 'j');
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
            result[0].Should().BeEquivalentTo('a', 'b', 'c');
        }
    }
}
