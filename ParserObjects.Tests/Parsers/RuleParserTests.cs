using System.Linq;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

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

            var input = FromString("abc");

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

            var input = FromString("abc");

            var result = target.Parse(input);
            result.Value.Should().Be("ab");
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void ValueTuple_Rule_2_Test()
        {
            var target = (_any, _any).Rule((a, b) => $"{a}{b}");

            var input = FromString("abc");

            target.Parse(input).Value.Should().Be("ab");
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

            var input = FromString("abc");

            target.Parse(input).Value.Should().Be("abc");
        }

        [Test]
        public void ValueTuple_Rule_3_Test()
        {
            var target = (_any, _any, _any).Rule((a, b, c) => $"{a}{b}{c}");

            var input = FromString("abc");

            target.Parse(input).Value.Should().Be("abc");
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

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcd");
        }

        [Test]
        public void ValueTuple_Rule_4_Test()
        {
            var target = (_any, _any, _any, _any).Rule((a, b, c, d) => $"{a}{b}{c}{d}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcd");
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

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcde");
        }

        [Test]
        public void ValueTuple_Rule_5_Test()
        {
            var target = (_any, _any, _any, _any, _any).Rule((a, b, c, d, e) => $"{a}{b}{c}{d}{e}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcde");
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

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdef");
        }

        [Test]
        public void ValueTuple_Rule_6_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any).Rule((a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdef");
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

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefg");
        }

        [Test]
        public void ValueTuple_Rule_7_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any).Rule((a, b, c, d, e, f, g) => $"{a}{b}{c}{d}{e}{f}{g}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefg");
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

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefgh");
        }

        [Test]
        public void ValueTuple_Rule_8_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any).Rule((a, b, c, d, e, f, g, h) => $"{a}{b}{c}{d}{e}{f}{g}{h}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefgh");
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

            var input = FromString("abcdefghijklmn");

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

            var input = FromString("abcdefghijklmn");

            var result = target.Parse(input);
            result.Value.Should().Be("abcdefghi");
            result.Consumed.Should().Be(9);
        }

        [Test]
        public void ValueTuple_Rule_9_Test()
        {
            var target = (_any, _any, _any, _any, _any, _any, _any, _any, _any).Rule((a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}");

            var input = FromString("abcdefghijklmn");

            target.Parse(input).Value.Should().Be("abcdefghi");
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
            var input = FromString("abd");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }
    }
}
