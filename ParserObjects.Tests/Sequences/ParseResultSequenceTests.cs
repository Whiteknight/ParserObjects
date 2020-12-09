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
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void GetNextPutBack_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("ac".ToCharacterSequence(), parser, null);
            target.GetNext().Value.Should().Be('a');
            target.PutBack(new SuccessResult<char>(null, 'b', null, 1));
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
        }

        [Test]
        public void Peek_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
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
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
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
            var target = new ParseResultSequence<char, char>("a".ToCharacterSequence(), parser, null);
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
            target.PutBack(new SuccessResult<char>(null, 'Y', null, 1));
            target.IsAtEnd.Should().BeFalse();
            target.GetNext();
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Location_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(2);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(3);
        }

        [Test]
        public void Location_PutBack()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(0);
            target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
            var b = target.GetNext();
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(2);
            target.PutBack(b);
            target.CurrentLocation.Line.Should().Be(1);
            target.CurrentLocation.Column.Should().Be(1);
        }

        [Test]
        public void Checkpoint_Test()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abcde".ToCharacterSequence(), parser, null);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
            cp.Rewind();
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_Putbacks()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abcde".ToCharacterSequence(), parser, null);
            target.GetNext().Value.Should().Be('a');
            target.GetNext().Value.Should().Be('b');
            target.PutBack(new SuccessResult<char>(null, 'Y', null, 1));
            target.PutBack(new SuccessResult<char>(null, 'X', null, 1));
            var cp = target.Checkpoint();
            target.GetNext().Value.Should().Be('X');
            target.GetNext().Value.Should().Be('Y');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
            cp.Rewind();
            target.GetNext().Value.Should().Be('X');
            target.GetNext().Value.Should().Be('Y');
            target.GetNext().Value.Should().Be('c');
            target.GetNext().Value.Should().Be('d');
            target.GetNext().Value.Should().Be('e');
            target.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Checkpoint_PutbacksIgnored()
        {
            var parser = new AnyParser<char>();
            var target = new ParseResultSequence<char, char>("abc".ToCharacterSequence(), parser, null);
            target.GetNext().Value.Should().Be('a');

            var cp = target.Checkpoint();
            target.PutBack(new SuccessResult<char>(null, 'Y', null, 1));
            target.PutBack(new SuccessResult<char>(null, 'X', null, 1));
            cp.Rewind();
            target.GetNext().Value.Should().Be('b');
            target.GetNext().Value.Should().Be('c');
            target.IsAtEnd.Should().BeTrue();
        }
    }
}
