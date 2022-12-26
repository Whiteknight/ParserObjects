using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class FromParseResultTests
    {
        [Test]
        public void GetNext_Test()
        {
            var parser = Any();
            var target = FromParseResult("abc".ToCharacterSequence(), parser);

            target.Consumed.Should().Be(0);
            target.GetNext().Value.Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Value.Should().Be('b');
            target.Consumed.Should().Be(2);
            target.GetNext().Value.Should().Be('c');
            target.Consumed.Should().Be(3);
            target.GetNext().Success.Should().BeFalse();
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void Reset_Test()
        {
            var parser = Any();
            var target = FromParseResult("abc".ToCharacterSequence(), parser);

            target.Consumed.Should().Be(0);
            target.GetNext().Value.Should().Be('a');
            target.Consumed.Should().Be(1);
            target.GetNext().Value.Should().Be('b');
            target.Consumed.Should().Be(2);
            target.Reset();
            target.Consumed.Should().Be(0);
            target.GetNext().Value.Should().Be('a');
        }

        [Test]
        public void Peek_Test()
        {
            var parser = Any();
            var target = FromParseResult("abc".ToCharacterSequence(), parser);
            target.Peek().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('a');
            target.Peek().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('b');
            target.Peek().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var parser = Any();
            var target = FromParseResult("abc".ToCharacterSequence(), parser);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Location_Test()
        {
            var parser = Any();
            var target = FromParseResult("abc".ToCharacterSequence(), parser);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var parser = Any();
            var target = FromParseResult("abcde".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
            cp.Rewind();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Rewind_Test()
        {
            var parser = Any();
            var target = FromParseResult("abcde".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
            target.Rewind(cp);
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void GetBetween_Test()
        {
            var parser = Any();
            var target = FromParseResult("abcdef".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp1 = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            var cp2 = target.Checkpoint();
            target.GetNext().Value.Should().Be('f');

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(3);
            result[0].Value.Should().Be('c');
            result[1].Value.Should().Be('d');
            result[2].Value.Should().Be('e');
        }

        [Test]
        public void GetBetween_OutOfOrder()
        {
            var parser = Any();
            var target = FromParseResult("abcdef".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp1 = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            var cp2 = target.Checkpoint();
            target.GetNext().Value.Should().Be('f');

            var result = target.GetBetween(cp2, cp1);
            result.Length.Should().Be(0);
        }

        [Test]
        public void GetBetween_Failure()
        {
            int count = 0;
            var values = "abcd";
            var parser = Function<char>(args =>
            {
                if (count > 4)
                    return args.Failure("Count too high");
                return args.Success(values[count++]);
            });
            var target = FromParseResult(FromString(""), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp1 = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            var cp2 = target.Checkpoint();

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(0);
        }
    }
}
