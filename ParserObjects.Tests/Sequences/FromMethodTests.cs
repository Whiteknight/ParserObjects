using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public static class FromMethodTests
{
    public class NonChars
    {
        [Test]
        public void GetNext_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return (0, true);
                return (idx + 1, idx == 2);
            });

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be(2);

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be(3);

            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be(0);

            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void Peek_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return (0, true);
                return (idx + 1, idx == 2);
            });

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.Peek().Should().Be(1);

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 };
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return (0, true);
                return (source[idx], idx == source.Length - 1);
            });

            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);

            var cp = target.Checkpoint();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);

            cp.Rewind();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);
        }

        [Test]
        public void GetBetween_Test()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 };
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return (0, true);
                return (source[idx], idx == source.Length - 1);
            });

            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);

            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);

            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be(5);
            target.GetNext().Should().Be(6);

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result.Should().ContainInOrder(3, 4);
        }
    }

    public class Chars
    {
        [Test]
        public void GetNext_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return ('\0', true);
                return ((char)('a' + idx), idx == 2);
            });

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be('a');

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be('b');

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be('c');

            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be('\0');

            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void GetNext_Normalized_LineColumn()
        {
            var source = "ab\r\ncd";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext().Should().Be('a');

            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext().Should().Be('b');

            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext().Should().Be('\n');

            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext().Should().Be('c');

            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext().Should().Be('d');

            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext().Should().Be('\0');

            target.CurrentLocation.Line.Should().Be(2);
            target.CurrentLocation.Column.Should().Be(2);
        }

        [Test]
        public void Peek_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return ('\0', true);
                return ((char)('a' + idx), idx == 2);
            });

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.Peek().Should().Be('a');

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');

            var cp = target.Checkpoint();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            cp.Rewind();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');
        }

        [Test]
        public void Checkpoint_DifferentBuffers()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            }, new SequenceOptions<char>
            {
                BufferSize = 2
            });

            target.GetNext().Should().Be('a');
            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('b');

            target.GetNext().Should().Be('c');
            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('d');

            target.GetNext().Should().Be('e');
            var cp3 = target.Checkpoint();
            target.GetNext().Should().Be('f');
            target.GetNext().Should().Be('\0');

            cp1.Rewind();
            target.GetNext().Should().Be('b');

            cp2.Rewind();
            target.GetNext().Should().Be('d');

            cp3.Rewind();
            target.GetNext().Should().Be('f');

            cp2.Rewind();
            target.GetNext().Should().Be('d');

            cp1.Rewind();
            target.GetNext().Should().Be('b');
        }

        [Test]
        public void GetBetween_Test()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');

            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('f');

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result.Should().ContainInOrder('c', 'd');
        }

        [Test]
        public void GetBetween_DifferentBuffers()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            }, new SequenceOptions<char>
            {
                BufferSize = 2
            });

            target.GetNext().Should().Be('a');
            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('b');

            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            target.GetNext().Should().Be('e');
            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('f');

            var result = target.GetBetween(cp1, cp2);
            result.Length.Should().Be(4);
            result.Should().ContainInOrder('b', 'c', 'd', 'e');
        }

        [Test]
        public void GetStringBetween_Test()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');

            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('f');
            target.GetNext().Should().Be('\0');

            var result = target.GetStringBetween(cp1, cp2);
            result.Should().Be("cd");
        }

        [Test]
        public void GetStringBetween_DifferentBuffers()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            }, new SequenceOptions<char>
            {
                BufferSize = 2
            });

            target.GetNext().Should().Be('a');
            var cp1 = target.Checkpoint();
            target.GetNext().Should().Be('b');

            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            target.GetNext().Should().Be('e');
            var cp2 = target.Checkpoint();
            target.GetNext().Should().Be('f');

            var result = target.GetStringBetween(cp1, cp2);
            result.Length.Should().Be(4);
            result.Should().Be("bcde");
        }
    }
}
