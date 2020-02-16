using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Sequences
{
    public class SequenceEnumerableTests
    {
        [Test]
        public void AsEnumerable_Test()
        {
            var source = new EnumerableSequence<int>(
                new[] { 1, 2, 3 },
                () => 0
            );
            var target = source.AsEnumerable().ToList();
            target[0].Should().Be(1);
            target[1].Should().Be(2);
            target[2].Should().Be(3);
        }
    }
}
