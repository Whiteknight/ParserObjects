using System.IO;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromByteStreamTests
{
    private static ISequence<byte> GetTarget(params byte[] b)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(b, 0, b.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return FromByteStream(memoryStream, new SequenceOptions<byte> { BufferSize = 5 });
    }

    [Test]
    public void GetNext_Test()
    {
        var target = GetTarget(1, 2, 3);

        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.Consumed.Should().Be(0);
        target.CurrentLocation.Column.Should().Be(0);

        target.GetNext().Should().Be(1);
        target.Consumed.Should().Be(1);
        target.CurrentLocation.Column.Should().Be(1);

        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(2);
        target.CurrentLocation.Column.Should().Be(2);

        target.GetNext().Should().Be(3);
        target.Consumed.Should().Be(3);
        target.CurrentLocation.Column.Should().Be(3);
        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeFalse();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeTrue();

        target.GetNext().Should().Be(0);
        target.Consumed.Should().Be(3);
        target.CurrentLocation.Column.Should().Be(3);
        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeFalse();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeTrue();
    }

    [Test]
    public void Peek_Test()
    {
        var target = GetTarget(1, 2, 3);
        target.Peek().Should().Be(1);
        target.Peek().Should().Be(1);
        target.Peek().Should().Be(1);
    }

    [Test]
    public void IsAtEnd_Test()
    {
        var target = GetTarget(1, 2, 3);
        target.IsAtEnd.Should().BeFalse();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.GetNext();
        target.IsAtEnd.Should().BeFalse();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.GetNext();
        target.IsAtEnd.Should().BeFalse();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.GetNext();
        target.IsAtEnd.Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeTrue();
    }

    [Test]
    public void Checkpoint_SameBuffer()
    {
        var target = GetTarget(1, 2, 3, 4, 5, 6);
        target.GetNext().Should().Be(1);
        var cp = target.Checkpoint();

        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        cp.Rewind();

        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(5);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);

        var stats = target.GetStatistics();
        stats.CheckpointsCreated.Should().Be(1);
        stats.Rewinds.Should().Be(1);
        stats.RewindsToCurrentBuffer.Should().Be(1);
    }

    [Test]
    public void Checkpoint_End()
    {
        var target = GetTarget(1, 2, 3, 4, 5, 6);
        target.GetNext().Should().Be(1);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(5);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
        target.IsAtEnd.Should().BeTrue();

        var cp = target.Checkpoint();
        cp.Rewind();
        target.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void Checkpoint_PreviousBuffer()
    {
        var target = GetTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        target.GetNext().Should().Be(1);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        var cp = target.Checkpoint();

        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(5);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(7);
        cp.Rewind();

        target.GetNext().Should().Be(4);

        var stats = target.GetStatistics();
        stats.CheckpointsCreated.Should().Be(1);
        stats.Rewinds.Should().Be(1);
        stats.RewindsToCurrentBuffer.Should().Be(0);
    }

    [Test]
    public void CustomEndSentinel_Test()
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(new byte[] { 1, 2, 3 }, 0, 3);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var target = FromByteStream(memoryStream, new SequenceOptions<byte> { BufferSize = 5, EndSentinel = 9 });
        target.GetNext().Should().Be(1);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.GetNext().Should().Be(9);
    }

    [Test]
    public void GetBetween_SameBuffer()
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 10);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var target = FromByteStream(memoryStream, new SequenceOptions<byte>
        {
            BufferSize = 20,
            EndSentinel = 0
        });

        target.GetNext(); // 1
        target.Consumed.Should().Be(1);
        target.GetNext(); // 2
        target.Consumed.Should().Be(2);

        var cp1 = target.Checkpoint();

        target.GetNext(); // 3
        target.Consumed.Should().Be(3);
        target.GetNext(); // 4
        target.Consumed.Should().Be(4);
        target.GetNext(); // 5
        target.Consumed.Should().Be(5);
        target.GetNext(); // 6
        target.Consumed.Should().Be(6);
        target.GetNext(); // 7
        target.Consumed.Should().Be(7);

        var cp2 = target.Checkpoint();

        target.GetNext(); // 8
        target.Consumed.Should().Be(8);
        target.GetNext(); // 9
        target.Consumed.Should().Be(9);
        target.GetNext(); // 10
        target.Consumed.Should().Be(10);

        var result = target.GetArrayBetween(cp1, cp2);
        result.Length.Should().Be(5);
        result.Should().ContainInOrder(3, 4, 5, 6, 7);
    }

    [Test]
    public void GetBetween_SeparateBuffers()
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 10);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var target = FromByteStream(memoryStream, new SequenceOptions<byte>
        {
            BufferSize = 3,
            EndSentinel = 9
        });

        target.GetNext(); // 1
        target.GetNext(); // 2

        var cp1 = target.Checkpoint();

        target.GetNext(); // 3
        target.GetNext(); // 4
        target.GetNext(); // 5
        target.GetNext(); // 6
        target.GetNext(); // 7

        var cp2 = target.Checkpoint();

        target.GetNext(); // 8
        target.GetNext(); // 9
        target.GetNext(); // 10

        var result = target.GetArrayBetween(cp1, cp2);
        result.Length.Should().Be(5);
        result.Should().ContainInOrder(3, 4, 5, 6, 7);
    }

    [Test]
    public void GetBetween_OutOfOrder()
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 10);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var target = FromByteStream(memoryStream, new SequenceOptions<byte>
        {
            BufferSize = 20,
            EndSentinel = 0
        });

        target.GetNext(); // 1
        target.GetNext(); // 2

        var cp1 = target.Checkpoint();

        target.GetNext(); // 3
        target.GetNext(); // 4
        target.GetNext(); // 5
        target.GetNext(); // 6
        target.GetNext(); // 7

        var cp2 = target.Checkpoint();

        target.GetNext(); // 8

        var result = target.GetArrayBetween(cp2, cp1);
        result.Length.Should().Be(0);
    }

    [Test]
    public void Reset_Test()
    {
        var target = GetTarget(1, 2, 3);

        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(1);
        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(2);

        target.Reset();
        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeFalse();
        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(1);
    }

    [Test]
    public void Reset_Empty()
    {
        var target = GetTarget();

        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeTrue();
        target.IsAtEnd.Should().BeTrue();
        target.Reset();
        target.IsAtEnd.Should().BeTrue();
        target.Flags.Has(SequenceStateType.StartOfInput).Should().BeTrue();
        target.Flags.Has(SequenceStateType.EndOfInput).Should().BeTrue();
    }
}
