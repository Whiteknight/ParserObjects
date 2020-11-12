using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class ParseResultSequenceTests
    {
        [Test]
        public void GetNext_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".AsCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("ac".AsCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.PutBack(Result.Success<char>('b', null));
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void Peek_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".AsCharacterSequence(), parser);
            target.Peek().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('a');
            target.Peek().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('b');
            target.Peek().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void Checkpoint_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abcde".AsCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.GetNext().Value.Should().Be('\0');
            cp.Rewind();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.GetNext().Value.Should().Be('\0');
        }

        [Test]
        public void Checkpoint_Putbacks()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abcde".AsCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.PutBack(Result.Success('Y'));
            target.PutBack(Result.Success('X'));
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('X');
            target.GetNext().Value.Should().Be('Y');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.GetNext().Value.Should().Be('\0');
            cp.Rewind();
            target.GetNext().Value.Should().Be('X');
            target.GetNext().Value.Should().Be('Y');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.GetNext().Value.Should().Be('\0');
        }

        [Test]
        public void Checkpoint_PutbacksIgnored()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".AsCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');

            var cp = target.Checkpoint();
            target.PutBack(Result.Success('Y'));
            target.PutBack(Result.Success('X'));
            cp.Rewind();
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('\0');
        }
    }
}
