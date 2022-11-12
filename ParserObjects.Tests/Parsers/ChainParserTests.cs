using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChainParserTests
    {
        [Test]
        public void Output_Callback_Basic()
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
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
            result.Consumed.Should().Be(2);

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Y');
            result.Consumed.Should().Be(2);

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("cZ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Z');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void Output_Callback_InitialFail()
        {
            var parser = Fail<object>().Chain(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(false);
        }

        [Test]
        public void Output_Callback_Throw()
        {
            var parser = Any().Chain<char, char, object>(c => throw new System.Exception());
            var input = new StringCharacterSequence("abc");
            Action act = () => parser.Parse(input);
            act.Should().Throw<Exception>();
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void Output_Callback_Null()
        {
            var parser = Any().Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Output_Chain_GetChildren_Test()
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
        public void Output_Chain_GetChildren_Mentions()
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
        public void NoOutput_Callback_Basic()
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
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
            result.Consumed.Should().Be(2);

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Y');
            result.Consumed.Should().Be(2);

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);

            result = parser.Parse("cZ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Z');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void NoOutput_Callback_InitialFail()
        {
            var parser = ((IParser<char>)Fail<object>()).Chain(c => Produce(() => c.Success));
            var result = parser.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(false);
        }

        [Test]
        public void NoOutput_Callback_Throw()
        {
            var parser = ((IParser<char>)Any()).Chain<char, object>(c => throw new System.Exception());
            var input = new StringCharacterSequence("abc");
            Action act = () => parser.Parse(input);
            act.Should().Throw<Exception>();
            input.GetNext().Should().Be('a');
        }

        [Test]
        public void NoOutput_Callback_Null()
        {
            var parser = ((IParser<char>)Any()).Chain(c => (IParser<char, string>)null);
            var result = parser.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void NoOutput_Chain_GetChildren_Test()
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
        public void NoOutput_Chain_GetChildren_Mentions()
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
    }
}
