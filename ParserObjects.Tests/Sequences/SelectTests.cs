using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class SelectTests
{
    [Test]
    public void GetNext_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x * 2);

        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(4);
        target.Consumed.Should().Be(2);
        target.GetNext().Should().Be(6);
        target.Consumed.Should().Be(3);
        target.GetNext().Should().Be(0);
        target.Consumed.Should().Be(3);
    }

    [Test]
    public void Peek_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x * 2);
        target.Peek().Should().Be(2);
        target.Peek().Should().Be(2);
        target.Peek().Should().Be(2);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(4);
    }

    [Test]
    public void Select_Test()
    {
        var source = FromList(
            new[] { 1, 2, 3 },
            0
        );
        var target = source.Select(x => x * 2);
        target.GetNext().Should().Be(2);
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
    }

    [Test]
    public void IsAtEnd_Test()
    {
        var source = FromList(
            new[] { 1, 2, 3 },
            0
        );
        var target = source.Select(x => x * 2);
        target.IsAtEnd.Should().BeFalse();
        target.GetNext().Should().Be(2);
        target.IsAtEnd.Should().BeFalse();
        target.GetNext().Should().Be(4);
        target.IsAtEnd.Should().BeFalse();
        target.GetNext().Should().Be(6);
        target.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void Location_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x);
        target.GetNext().Should().Be(1);
        target.CurrentLocation.Column.Should().Be(1);
        target.GetNext().Should().Be(2);
        target.CurrentLocation.Column.Should().Be(2);
        target.GetNext().Should().Be(3);
        target.CurrentLocation.Column.Should().Be(3);
        target.GetNext().Should().Be(0);
        target.CurrentLocation.Column.Should().Be(3);
    }

    [Test]
    public void Checkpoint_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x * 2);
        target.GetNext().Should().Be(2);
        var cp = target.Checkpoint();
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
        cp.Rewind();
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
    }

    [Test]
    public void Checkpoint_IsAtEndRewind()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x * 2);
        target.GetNext().Should().Be(2);
        var cp = target.Checkpoint();
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
        target.IsAtEnd.Should().BeTrue();
        cp.Rewind();
        target.IsAtEnd.Should().BeFalse();
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(0);
    }

    [Test]
    public void GetBetween_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3, 4, 5, 6 },
            0
        ).Select(x => x * 2);
        target.GetNext().Should().Be(2);
        var cp1 = target.Checkpoint();
        target.GetNext().Should().Be(4);
        target.GetNext().Should().Be(6);
        target.GetNext().Should().Be(8);
        var cp2 = target.Checkpoint();
        target.GetNext().Should().Be(10);

        var result = target.GetBetween(cp1, cp2);
        result.Length.Should().Be(3);
        result.Should().ContainInOrder(4, 6, 8);

        target.GetNext().Should().Be(12);
    }

    [Test]
    public void Reset_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Select(x => x * 2);

        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(4);
        target.Consumed.Should().Be(2);

        target.Reset();
        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(2);
    }
}
