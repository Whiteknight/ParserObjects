using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromEnumerableTests
{
    [Test]
    public void GetNext_Test()
    {
        var target = FromEnumerable(new[] { 1, 2, 3 });
        target.GetNext().Should().Be(1);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.IsAtEnd.Should().BeTrue();
        target.Consumed.Should().Be(3);

        var stats = target.GetStatistics();
        stats.BufferRefills.Should().Be(0);
        stats.ItemsRead.Should().Be(3);
    }

    [Test]
    public void Checkpoint_Test()
    {
        var target = FromEnumerable(new[] { 1, 2, 3 });
        target.GetNext().Should().Be(1);
        var cp = target.Checkpoint();
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.Consumed.Should().Be(3);
        cp.Rewind();
        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(3);
        target.Consumed.Should().Be(3);

        var stats = target.GetStatistics();
        stats.Rewinds.Should().Be(1);
        stats.RewindsToCurrentBuffer.Should().Be(1);
        stats.BufferRefills.Should().Be(0);
        stats.ItemsRead.Should().Be(5);
    }
}
