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
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("ac".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.PutBack(new Result<char>(null, true, 'b', null, null));
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void Peek_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser);
            target.Peek().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('a');
            target.Peek().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('b');
            target.Peek().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void IsAtEnd_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser);
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void IsAtEnd_PutBack()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("a".ToCharacterSequence(), parser);
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
            target.PutBack(new Result<char>(null, true, 'Y', null, null));
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abcde".ToCharacterSequence(), parser);
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
            var target = new ParseResultSequence<char, char>("abcde".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.PutBack(new Result<char>(null, true, 'Y', null, null));
            target.PutBack(new Result<char>(null, true, 'X', null, null));
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
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser);
            target.GetNext().Value.Should().Be('a');

            var cp = target.Checkpoint();
            target.PutBack(new Result<char>(null, true, 'Y', null, null));
            target.PutBack(new Result<char>(null, true, 'X', null, null));
            cp.Rewind();
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('\0');
        }
    }
}
