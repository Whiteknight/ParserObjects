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
    }
}
