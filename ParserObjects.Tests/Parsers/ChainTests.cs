﻿using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class ChainTests
{
    public class Output
    {
        [TestCase("aX", 'X', true)]
        [TestCase("bY", 'Y', true)]
        [TestCase("aY", ' ', false)]
        [TestCase("bX", ' ', false)]
        [TestCase("cC", ' ', false)]
        [TestCase("cZ", 'Z', true)]
        [TestCase("dW", ' ', false)]
        public void Parse_Test(string test, char resultValue, bool shouldMatch)
        {
            var parser = Any().Chain(r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });

            var result = parser.Parse(test);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? test.Length : 0);
            if (shouldMatch)
                result.Value.Should().Be(resultValue);
        }

        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("aY", false)]
        [TestCase("bX", false)]
        [TestCase("cC", false)]
        [TestCase("cZ", true)]
        [TestCase("dW", false)]
        public void Match_Test(string test, bool shouldMatch)
        {
            var parser = Any().Chain(r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });

            var input = FromString(test);
            var result = parser.Match(input);
            result.Should().Be(shouldMatch);
            input.Consumed.Should().Be(shouldMatch ? test.Length : 0);
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = Fail<object>().Chain(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(false);
        }

        [Test]
        public void Parse_Throw()
        {
            var parser = Any().Chain<char, char, object>(c => throw new System.Exception());
            var input = FromString("abc");
            Action act = () => parser.Parse(input);
            act.Should().Throw<Exception>();
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Parse_NullParser()
        {
            var parser = Any().Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_NullParser()
        {
            var parser = Any().Chain(c => (IParser<char, string>)null);
            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var first = Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = Chain(first, r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return x;
                if (c == 'b')
                    return y;
                return z;
            });

            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result.Should().Contain(first);
        }

        [Test]
        public void GetChildren_Mentions()
        {
            var first = Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = Chain(first, r =>
            {
                var c = r.Value;
                if (c == 'a')
                    return x;
                if (c == 'b')
                    return y;
                return z;
            }, x, y, z);

            var result = target.GetChildren().ToList();
            result.Count.Should().Be(4);
            result.Should().Contain(first);
            result.Should().Contain(x);
            result.Should().Contain(y);
            result.Should().Contain(z);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Any().Chain(c => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .->CHAIN");
        }
    }

    public class NoOutput
    {
        [TestCase("aX", 'X', true)]
        [TestCase("bY", 'Y', true)]
        [TestCase("aY", ' ', false)]
        [TestCase("bX", ' ', false)]
        [TestCase("cC", ' ', false)]
        [TestCase("cZ", 'Z', true)]
        [TestCase("dW", ' ', false)]
        public void Parse_Test(string test, char resultValue, bool shouldMatch)
        {
            var parser = ((IParser<char>)Any()).Chain(r =>
            {
                var c = r.Value;
                if ((char)c == 'a')
                    return Match('X');
                if ((char)c == 'b')
                    return Match('Y');
                return Match('Z');
            });

            var result = parser.Parse(test);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? test.Length : 0);
            if (shouldMatch)
                result.Value.Should().Be(resultValue);
        }

        [Test]
        public void Parse_InitialFail()
        {
            var parser = ((IParser<char>)Fail<object>()).Chain(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(false);
        }

        [Test]
        public void Parse_Throw()
        {
            var parser = ((IParser<char>)Any()).Chain<char, object>(c => throw new System.Exception());
            var input = FromString("abc");
            Action act = () => parser.Parse(input);
            act.Should().Throw<Exception>();
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Parse_Null()
        {
            var parser = ((IParser<char>)Any()).Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("aY", false)]
        [TestCase("bX", false)]
        [TestCase("cC", false)]
        [TestCase("cZ", true)]
        [TestCase("dW", false)]
        public void Match_Test(string test, bool shouldMatch)
        {
            var parser = ((IParser<char>)Any()).Chain(r =>
            {
                var c = (char)r.Value;
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });

            var input = FromString(test);
            var result = parser.Match(input);
            result.Should().Be(shouldMatch);
            input.Consumed.Should().Be(shouldMatch ? test.Length : 0);
        }

        [Test]
        public void Match_NullParser()
        {
            var parser = ((IParser<char>)Any()).Chain(c => (IParser<char, string>)null);
            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var first = (IParser<char>)Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = Chain(first, r =>
            {
                var c = r.Value;
                if ((char)c == 'a')
                    return x;
                if ((char)c == 'b')
                    return y;
                return z;
            });

            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result.Should().Contain(first);
        }

        [Test]
        public void GetChildren_Mentions()
        {
            var first = (IParser<char>)Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = Chain(first, r =>
            {
                var c = r.Value;
                if ((char)c == 'a')
                    return x;
                if ((char)c == 'b')
                    return y;
                return z;
            }, x, y, z);

            var result = target.GetChildren().ToList();
            result.Count.Should().Be(4);
            result.Should().Contain(first);
            result.Should().Contain(x);
            result.Should().Contain(y);
            result.Should().Contain(z);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Empty().Chain(c => Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := ()->CHAIN");
        }
    }
}
