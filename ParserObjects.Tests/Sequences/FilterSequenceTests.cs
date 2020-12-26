using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class FilterSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var target = new FilterSequence<int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3, 4, 5, 6 },
                    () => 0
                ),
                x => x % 2 == 0
            );
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var target = new FilterSequence<int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3 },
                    () => 0
                ),
                x => x % 2 == 0
            );
            target.IsAtEnd.Should().BeFalse();
            target.GetNext().Should().Be(2);
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Peek_Test()
        {
            var target = new FilterSequence<int>(
                new EnumerableSequence<int>(
                    new[] { 1, 2, 3, 4, 5, 6 },
                    () => 0
                ),
                x => x % 2 == 0
            );
            target.Peek().Should().Be(2);
            target.GetNext().Should().Be(2);
            target.Peek().Should().Be(4);
            target.GetNext().Should().Be(4);
            target.Peek().Should().Be(6);
            target.GetNext().Should().Be(6);
            target.Peek().Should().Be(0);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Where_Test()
        {
            var source = new EnumerableSequence<int>(
                new[] { 1, 2, 3, 4, 5, 6 },
                () => 0
            );
            var target = source.Where(x => x % 2 == 0);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(4);
            target.GetNext().Should().Be(6);
            target.GetNext().Should().Be(0);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var target = new FilterSequence<char>(
                "aBcDeFgH".ToCharacterSequence(),
                x => char.IsUpper(x)
            );
            target.GetNext().Should().Be('B');
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('D');
            target.GetNext().Should().Be('F');
            target.GetNext().Should().Be('H');
            target.GetNext().Should().Be('\0');
            cp.Rewind();
            target.GetNext().Should().Be('D');
            target.GetNext().Should().Be('F');
            target.GetNext().Should().Be('H');
            target.GetNext().Should().Be('\0');
        }
    }
}
