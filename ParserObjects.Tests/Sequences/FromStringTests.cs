using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromStringTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void GetNext_Test(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetNext().Should().Be('a');
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.GetNext().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetNext_Flags(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Peek_Test(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Peek().Should().Be('a');
        target.Peek().Should().Be('a');
        target.Peek().Should().Be('a');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Peek_DoesNotChangeFlags(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.Peek().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.Peek().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.Peek().Should().Be('a');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Peek_End(bool normalize)
    {
        var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Peek().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EmptyFlags(bool normalize)
    {
        var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
        target.Peek().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetRemainder_Test(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetNext().Should().Be('a');
        target.GetRemainder().Should().Be("bc");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetRemainder_Full(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetRemainder().Should().Be("abc");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetRemainder_Empty(bool normalize)
    {
        var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetRemainder().Should().Be("");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetRemainder_End(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetNext();
        target.GetNext();
        target.GetNext();
        target.GetRemainder().Should().Be("");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetNext_Empty(bool normalize)
    {
        var target = FromString("", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        // Every get attempt past the end of the string will return '\0'
        target.GetNext().Should().Be('\0');
        target.GetNext().Should().Be('\0');
        target.GetNext().Should().Be('\0');
        target.GetNext().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetNext_Empty_CustomEndSentinel(bool normalize)
    {
        var target = FromString("", new SequenceOptions<char>
        {
            EndSentinel = 'X',
            MaintainLineEndings = !normalize
        });
        // Every get attempt past the end of the string will return '\0'
        target.GetNext().Should().Be('X');
        target.GetNext().Should().Be('X');
        target.GetNext().Should().Be('X');
        target.GetNext().Should().Be('X');
    }

    [Test]
    public void GetNext_WindowsNewlines()
    {
        var target = FromString("\r\na\r\n");
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeFalse();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
    }

    [Test]
    public void GetNext_WindowsNewlines_NonNormalized()
    {
        var target = FromString("\r\na\r\n", new SequenceOptions<char>
        {
            MaintainLineEndings = true
        });
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\r');
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeFalse();
        target.GetNext().Should().Be('\r');
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
    }

    [Test]
    public void GetNext_UnixNewlines()
    {
        var target = FromString("\na\n");
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeFalse();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
    }

    [Test]
    public void GetNext_OldMacNewlines()
    {
        var target = FromString("\ra\r");
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeFalse();
        target.GetNext().Should().Be('\n');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
    }

    [Test]
    public void GetNext_OldMacNewlines_NonNormalized()
    {
        var target = FromString("\ra\r", new SequenceOptions<char>
        {
            MaintainLineEndings = true
        });
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeTrue();
        target.GetNext().Should().Be('\r');
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfLine).Should().BeFalse();
        target.GetNext().Should().Be('\r');
        target.GetNext().Should().Be('\0');
    }

    [Test]
    public void GetNext_N()
    {
        var target = FromString("abcdef");
        var result = target.GetNext(4);
        result.Length.Should().Be(4);
        result.Should().ContainInOrder('a', 'b', 'c', 'd');
        target.Consumed.Should().Be(4);
    }

    [Test]
    public void GetNext_N_Negative()
    {
        var target = FromString("abcdef");
        var act = () => target.GetNext(-4);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetString_Test()
    {
        var target = FromString("abcdef");
        var result = target.GetString(4);
        result.Should().Be("abcd");
        target.Consumed.Should().Be(4);
    }

    [Test]
    public void Peek_N()
    {
        var target = FromString("abcdef");
        var result = target.Peek(4);
        result.Length.Should().Be(4);
        result.Should().ContainInOrder('a', 'b', 'c', 'd');
        target.Consumed.Should().Be(0);
    }

    [Test]
    public void Peek_N_Negative()
    {
        var target = FromString("abcdef");
        var act = () => target.Peek(-4);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void PeekString_Test()
    {
        var target = FromString("abcdef");
        var result = target.PeekString(4);
        result.Should().Be("abcd");
        target.Consumed.Should().Be(0);
    }

    [Test]
    public void Peek_WindowsNewlines()
    {
        var target = FromString("\r\na\r\n");
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('a');
        target.GetNext();
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('\0');
    }

    [Test]
    public void Peek_UnixNewlines()
    {
        var target = FromString("\na\n");
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('a');
        target.GetNext();
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('\0');
    }

    [Test]
    public void Peek_OldMacNewlines()
    {
        var target = FromString("\ra\r");
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('a');
        target.GetNext();
        target.Peek().Should().Be('\n');
        target.GetNext();
        target.Peek().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Location_Test(bool normalize)
    {
        var target = FromString("a\nbc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetNext().Should().Be('a');
        target.CurrentLocation.Line.Should().Be(1);
        target.CurrentLocation.Column.Should().Be(1);

        target.GetNext().Should().Be('\n');

        target.GetNext().Should().Be('b');
        target.CurrentLocation.Line.Should().Be(2);
        target.CurrentLocation.Column.Should().Be(1);

        target.GetNext().Should().Be('c');
        target.CurrentLocation.Line.Should().Be(2);
        target.CurrentLocation.Column.Should().Be(2);

        target.GetNext().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Location_Rewind(bool normalize)
    {
        var target = FromString("abc\nde", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.GetNext();
        target.GetNext();
        target.GetNext();
        target.CurrentLocation.Line.Should().Be(1);
        target.CurrentLocation.Column.Should().Be(3);
        var checkpoint = target.Checkpoint();

        target.GetNext().Should().Be('\n');
        target.CurrentLocation.Line.Should().Be(2);
        target.CurrentLocation.Column.Should().Be(0);

        checkpoint.Rewind();
        target.CurrentLocation.Line.Should().Be(1);
        target.CurrentLocation.Column.Should().Be(3);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Reset_Test(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
        target.Reset();
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.GetNext().Should().Be('\0');
    }

    [TestCase(true)]
    [TestCase(false)]
    public void IsAtEnd_Test(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.IsAtEnd.Should().BeFalse();
        target.GetNext();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.IsAtEnd.Should().BeFalse();
        target.GetNext();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        target.IsAtEnd.Should().BeFalse();
        target.GetNext();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeTrue();
        target.IsAtEnd.Should().BeTrue();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Checkpoint_Beginning(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char> { MaintainLineEndings = !normalize });
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateTypes.EndOfInput).Should().BeFalse();
        var cp = target.Checkpoint();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.GetNext().Should().Be('\0');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
        cp.Rewind();
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeTrue();
        target.GetNext().Should().Be('a');
        target.Flags.Has(SequenceStateTypes.StartOfInput).Should().BeFalse();
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        target.GetNext().Should().Be('\0');
    }

    [Test]
    public void ReadNewlineIncrementsConsumed()
    {
        var target = FromString("\r\n");
        var next = target.GetNext();
        next.Should().Be('\n');
        target.Consumed.Should().Be(1);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetStatistics(bool normalize)
    {
        var target = FromString("abc", new SequenceOptions<char>
        {
            MaintainLineEndings = !normalize
        });
        // Every get attempt past the end of the string will return '\0'
        target.GetNext().Should().Be('a');
        target.GetNext().Should().Be('b');
        var stats = target.GetStatistics();
        stats.ItemsRead.Should().Be(2);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetBetween(bool normalize)
    {
        var target = FromString("abcd", new SequenceOptions<char>
        {
            MaintainLineEndings = !normalize
        });
        // Every get attempt past the end of the string will return '\0'
        target.GetNext().Should().Be('a');
        var cp1 = target.Checkpoint();
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        var cp2 = target.Checkpoint();

        var result = target.GetBetween(cp1, cp2, (object)null, (d, _) => new string(d));
        result.Should().Be("bc");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetStringBetween(bool normalize)
    {
        var target = FromString("abcd", new SequenceOptions<char>
        {
            MaintainLineEndings = !normalize
        });
        // Every get attempt past the end of the string will return '\0'
        target.GetNext().Should().Be('a');
        var cp1 = target.Checkpoint();
        target.GetNext().Should().Be('b');
        target.GetNext().Should().Be('c');
        var cp2 = target.Checkpoint();

        var result = target.GetStringBetween(cp1, cp2);
        result.Should().Be("bc");
    }
}
