using System.Linq;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FailParserTests
    {
        private IParser<char, char> SingleInstance() => Fail();

        private IMultiParser<char, char> MultiInstance() => FailMulti<char>();

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("a")]
        [TestCase("1")]
        [TestCase(".")]
        public void Parse_Test(string test)
        {
            var parser = SingleInstance();
            parser.CanMatch(test).Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("a")]
        [TestCase("1")]
        [TestCase(".")]
        public void Parse_Multi(string test)
        {
            var parser = MultiInstance();
            parser.CanMatch(test).Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Fail<object>();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
