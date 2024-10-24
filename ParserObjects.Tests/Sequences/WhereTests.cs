using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class WhereTests
{
    [Test]
    public void GetNext_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3, 4, 5, 6 },
            0
        ).Where(x => x % 2 == 0);

        // .Consumed is based on the value from the underlying sequence, so we have to acknowledge
        // that we are skipping values
        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(2);
        target.GetNext().Should().Be(4);
        target.Consumed.Should().Be(4);
        target.GetNext().Should().Be(6);
        target.Consumed.Should().Be(6);
        target.GetNext().Should().Be(0);
        target.Consumed.Should().Be(6);
    }

    [Test]
    public void IsAtEnd_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3 },
            0
        ).Where(x => x % 2 == 0);
        target.IsAtEnd.Should().BeFalse();
        target.GetNext().Should().Be(2);
        target.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void IsAtEnd_Empty()
    {
        var target = FromList(
            new int[] { },
            0
        ).Where(x => x % 2 == 0);
        target.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void IsAtEnd_NoMatches()
    {
        var target = FromList(
            new int[] { 1, 3, 5 },
            0
        ).Where(x => x % 2 == 0);
        target.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void Peek_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3, 4, 5, 6 },
            0
        ).Where(x => x % 2 == 0);
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
        var source = FromList(
            new[] { 1, 2, 3, 4, 5, 6 },
            0
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
        var target = "aBcDeFgH".ToCharacterSequence().Where(x => char.IsUpper(x));
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

    [Test]
    public void Reset_Test()
    {
        var target = FromList(
            new[] { 1, 2, 3, 4, 5, 6 },
            0
        ).Where(x => x % 2 == 0);

        target.Consumed.Should().Be(1);
        target.GetNext().Should().Be(2);
        target.Consumed.Should().Be(2);
        target.GetNext().Should().Be(4);
        target.Consumed.Should().Be(4);

        target.Reset();
        target.Consumed.Should().Be(0);
        target.GetNext().Should().Be(2);
    }
}
