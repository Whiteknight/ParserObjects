using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class RuleParserTests
    {
        private readonly IParser<char, char> _any = Any();

        [Test]
        public void Rule_2_Test()
        {
            var target = Rule(
                _any,
                _any,
                (a, b) => $"{a}{b}"
            );

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("ab");
        }

        [Test]
        public void Rule_2_Consumed()
        {
            var target = Rule(
                _any,
                _any,
                (a, b) => $"{a}{b}"
            );

            var input = new StringCharacterSequence("abc");

            var result = target.Parse(input);
            result.Value.Should().Be("ab");
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void ValueTuple_Produce_2_Test()
        {
            var target = (_any, _any).Produce((a, b) => $"{a}{b}");

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("ab");
        }

        [Test]
        public void ValueTuple_Combine_2_Test()
        {
            var target = (_any, _any).Combine();

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Count.Should().Be(2);
        }

        [Test]
        public void Rule_3_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                (a, b, c) => $"{a}{b}{c}"
            );

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("abc");
        }

        [Test]
        public void ValueTuple_Produce_3_Test()
        {
            var target = (_any, _any, _any).Produce((a, b, c) => $"{a}{b}{c}");

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("abc");
        }

        [Test]
        public void ValueTuple_Combine_3_Test()
        {
            var target = (_any, _any, _any).Combine();

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Count.Should().Be(3);
        }

        [Test]
        public void Rule_4_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d) => $"{a}{b}{c}{d}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcd");
        }

        [Test]
        public void ValueTuple_Produce_4_Test()
        {
            var target = (_any, _any, _any, _any).Produce((a, b, c, d) => $"{a}{b}{c}{d}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcd");
        }

        [Test]
        public void ValueTuple_Combine_4_Test()
        {
            var target = (_any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(4);
        }

        [Test]
        public void Rule_5_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcde");
        }

        [Test]
        public void ValueTuple_Produce_5_Test()
        {
            var target = (_any, _any, _any, _any, _any).Produce((a, b, c, d, e) => $"{a}{b}{c}{d}{e}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcde");
        }

        [Test]
        public void ValueTuple_Combine_5_Test()
        {
            var target = (_any, _any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(5);
        }

        [Test]
        public void Rule_6_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdef");
        }

        [Test]
        public void ValueTuple_Produce_6_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any).Produce((a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdef");
        }

        [Test]
        public void ValueTuple_Combine_6_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(6);
        }

        [Test]
        public void Rule_7_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e, f, g) => $"{a}{b}{c}{d}{e}{f}{g}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefg");
        }

        [Test]
        public void ValueTuple_Produce_7_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any).Produce((a, b, c, d, e, f, g) => $"{a}{b}{c}{d}{e}{f}{g}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefg");
        }

        [Test]
        public void ValueTuple_Combine_7_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(7);
        }

        [Test]
        public void Rule_8_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e, f, g, h) => $"{a}{b}{c}{d}{e}{f}{g}{h}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefgh");
        }

        [Test]
        public void ValueTuple_Produce_8_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any).Produce((a, b, c, d, e, f, g, h) => $"{a}{b}{c}{d}{e}{f}{g}{h}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefgh");
        }

        [Test]
        public void ValueTuple_Combine_8_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            target.Parse(input).Value.Count.Should().Be(8);
        }

        [Test]
        public void Rule_9_Test()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefghi");
        }

        [Test]
        public void Rule_9_Consumed()
        {
            var target = Rule(
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                _any,
                (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
            );

            var input = new StringCharacterSequence("abcdefghijklmn");

            var result = target.Parse(input);
            result.Value.Should().Be("abcdefghi");
            result.Consumed.Should().Be(9);
        }

        [Test]
        public void ValueTuple_Produce_9_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any, _any).Produce((a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}");

            var input = new StringCharacterSequence("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefghi");
        }

        [Test]
        public void ValueTuple_Combine_9_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any, _any).Combine();

            var input = new StringCharacterSequence("abcdefghi");

            var result = target.Parse(input).Value;
            result.Count.Should().Be(9);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();

            var target = Rule(
                _any,
                failParser,
                (a, b) => $"{a}{b}"
            );

            var result = target.GetChildren().ToList();
            result[0].Should().BeSameAs(_any);
            result[1].Should().BeSameAs(failParser);
        }

        [Test]
        public void RewindInput_Test()
        {
            var parser = Rule(
                Match('a'),
                Match('b'),
                Match('c'),
                (a, b, c) => "ok"
            );
            var input = new StringCharacterSequence("abd");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }
    }
}
