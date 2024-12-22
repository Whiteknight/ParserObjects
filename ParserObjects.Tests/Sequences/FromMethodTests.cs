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

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(1);
            target.GetNext().Should().Be(2);

            target.CurrentLocation.Column.Should().NotBe(0);

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(2);
            target.GetNext().Should().Be(3);

            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
            target.GetNext().Should().Be(0);

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
            target.IsAtEnd.Should().BeTrue();
            target.Consumed.Should().Be(3);
        }

        [Test]
        public void Peek_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx >= 2)
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
        public void Peek_EndSentinel()
        {
            var target = FromMethod(idx =>
            {
                if (idx >= 0)
                    return (0, true);
                return (idx + 1, idx >= 0);
            });

            target.Peek().Should().Be(0);
            target.GetNext();
            target.Peek().Should().Be(0);
        }

        [Test]
        public void Peek_DoesNotChangeFlags()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return (0, true);
                return (idx + 1, idx == 2);
            });

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.Peek().Should().Be(1);

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
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
        public void Checkpoint_Beginning()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 };
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return (0, true);
                return (source[idx], idx == source.Length - 1);
            });

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            var cp = target.Checkpoint();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.GetNext().Should().Be(3);
            target.GetNext().Should().Be(4);

            cp.Rewind();
            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
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

            var result = target.GetArrayBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result.Should().ContainInOrder(3, 4);
        }

        [Test]
        public void GetBetween_WrongOrder()
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

            var result = target.GetArrayBetween(cp2, cp1);
            result.Length.Should().Be(0);
        }

        [Test]
        public void GetStatistics_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return (0, true);
                return (idx + 1, idx == 2);
            });

            var result = target.GetStatistics();
            result.ItemsRead.Should().Be(0);
            result.ItemsGenerated.Should().Be(1);
        }

        [Test]
        public void Reset_Test()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return (0, true);
                return (idx + 1, idx == 2);
            });

            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
            target.Reset();
            target.Consumed.Should().Be(0);
            target.GetNext().Should().Be(1);
            target.GetNext().Should().Be(2);
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
        public void Peek_NormalizeLineEndings()
        {
            var target = FromMethod(idx =>
            {
                if (idx >= 4)
                    return ('\0', true);
                return ("\r\n\r\n"[idx], idx >= 4);
            });

            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);
            target.Peek().Should().Be('\n');
        }

        [Test]
        public void Peek_DoesNotChangeFlags()
        {
            var target = FromMethod(idx =>
            {
                if (idx > 2)
                    return ('\0', true);
                return ((char)('a' + idx), idx == 2);
            });

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.IsAtEnd.Should().BeFalse();
            target.Consumed.Should().Be(0);

            target.Peek().Should().Be('a');

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
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
        public void Checkpoint_Beginning()
        {
            var source = "abcdef";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            var cp = target.Checkpoint();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            cp.Rewind();
            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
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

            var result = target.GetArrayBetween(cp1, cp2);
            result.Length.Should().Be(2);
            result.Should().ContainInOrder('c', 'd');
        }

        [Test]
        public void GetBetween_WrongOrder()
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

            var result = target.GetArrayBetween(cp2, cp1);
            result.Length.Should().Be(0);
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

            var result = target.GetArrayBetween(cp1, cp2);
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

        [Test]
        public void GetRemainder_Test()
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

            var result = target.GetRemainder();
            result.Should().Be("cdef");
        }

        [Test]
        public void GetRemainder_KnowWhereEndIs()
        {
            // There may be a small performance boost if we already know where the end is, so we
            // can allocate exactly the right size array. This test covers those cases.
            var source = "abcd";
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
            target.GetNext().Should().Be('b');
            var cp1 = target.Checkpoint();

            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            var result = target.GetRemainder();
            result.Should().Be("");

            cp1.Rewind();
            result = target.GetRemainder();
            result.Should().Be("cd");
        }

        [Test]
        public void Reset_Test()
        {
            var source = "abc";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');

            target.Reset();
            target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
            target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();

            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
        }

        [Test]
        public void GetStatistics_Test()
        {
            var source = "abc";
            var target = FromMethod(idx =>
            {
                if (idx >= source.Length)
                    return ('\0', true);
                return (source[idx], idx == source.Length - 1);
            });

            var result = target.GetStatistics();
            result.ItemsRead.Should().Be(0);
        }
    }
}
