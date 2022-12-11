using System.IO;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromCharacterFileTests
{
    private void Test(string content, Action<ISequence<char>> act)
    {
        var fileName = Guid.NewGuid().ToString() + ".txt";
        ISequence<char> target = null;
        try
        {
            File.WriteAllText(fileName, content);
            target = FromCharacterFile(fileName);
            act(target);
        }
        finally
        {
            (target as IDisposable)?.Dispose();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [Test]
    public void GetNext_Test()
    {
        Test("test", target =>
        {
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('s');
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('\0');
        });
    }

    [Test]
    public void Peek_Test()
    {
        Test("test", target =>
        {
            target.Peek().Should().Be('t');
            target.Peek().Should().Be('t');
            target.Peek().Should().Be('t');
        });
    }

    [Test]
    public void CurrentLocation_Test()
    {
        Test("te\nst", target =>
        {
            target.GetNext().Should().Be('t');
            target.CurrentLocation.Column.Should().Be(1);
            target.CurrentLocation.Line.Should().Be(1);
            target.Consumed.Should().Be(1);

            target.GetNext().Should().Be('e');
            target.CurrentLocation.Column.Should().Be(2);
            target.CurrentLocation.Line.Should().Be(1);
            target.Consumed.Should().Be(2);

            target.GetNext().Should().Be('\n');
            target.CurrentLocation.Column.Should().Be(0);
            target.CurrentLocation.Line.Should().Be(2);
            target.Consumed.Should().Be(3);

            target.GetNext().Should().Be('s');
            target.CurrentLocation.Column.Should().Be(1);
            target.CurrentLocation.Line.Should().Be(2);
            target.Consumed.Should().Be(4);

            target.GetNext().Should().Be('t');
            target.CurrentLocation.Column.Should().Be(2);
            target.CurrentLocation.Line.Should().Be(2);
            target.Consumed.Should().Be(5);

            target.GetNext().Should().Be('\0');
            target.CurrentLocation.Column.Should().Be(2);
            target.CurrentLocation.Line.Should().Be(2);
            target.Consumed.Should().Be(5);
        });
    }

    [Test]
    public void Rewind_Test()
    {
        Test("test", target =>
        {
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('e');
            var cp = target.Checkpoint();

            target.GetNext().Should().Be('s');
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('\0');
            target.CurrentLocation.Column.Should().Be(4);
            target.Consumed.Should().Be(4);

            cp.Rewind();
            target.Peek().Should().Be('s');
            target.CurrentLocation.Column.Should().Be(2);
            target.Consumed.Should().Be(2);
        });
    }
}
