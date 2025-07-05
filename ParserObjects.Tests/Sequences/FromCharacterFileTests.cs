using System.IO;
using System.Text;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences;

public class FromCharacterFileTests
{
    private static void Test(string content, SequenceOptions<char> options, Action<ICharSequence> act)
    {
        var fileName = Guid.NewGuid().ToString() + ".txt";
        ICharSequence target = null;
        try
        {
            File.WriteAllText(fileName, content);
            target = FromCharacterFile(options with { FileName = fileName });
            act(target);
        }
        finally
        {
            (target as IDisposable)?.Dispose();
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void GetNext_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("test", options, target =>
        {
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('e');
            target.GetNext().Should().Be('s');
            target.GetNext().Should().Be('t');
            target.GetNext().Should().Be('\0');
        });
    }

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void Peek_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("test", options, target =>
        {
            target.Peek().Should().Be('t');
            target.Peek().Should().Be('t');
            target.Peek().Should().Be('t');
        });
    }

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void CurrentLocation_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("te\nst", options, target =>
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

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void Rewind_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("test", options, target =>
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

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void GetRemainder_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("abcdefghi", options, target =>
        {
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            var result = target.GetRemainder();
            result.Should().Be("efghi");

            target.GetNext().Should().Be('e');
        });
    }

    [TestCase(5, true)]
    [TestCase(5, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    public void Reset_Test(int bufferSize, bool isAscii)
    {
        var options = new SequenceOptions<char>
        {
            BufferSize = bufferSize,
            Encoding = isAscii ? Encoding.ASCII : Encoding.UTF8
        };

        Test("abcdefghi", options, target =>
        {
            target.GetNext().Should().Be('a');
            target.GetNext().Should().Be('b');
            target.GetNext().Should().Be('c');
            target.GetNext().Should().Be('d');

            target.Reset();
            target.Consumed.Should().Be(0);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext().Should().Be('a');
        });
    }
}
