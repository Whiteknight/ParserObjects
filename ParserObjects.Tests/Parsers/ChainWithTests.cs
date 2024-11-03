using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class ChainWithTests
{
    public class Function
    {
        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("cZ", true)]
        [TestCase("aZ", false)]
        [TestCase("dW", false)]
        public void Parse_Test(string test, bool shouldMatch)
        {
            var target = ChainWith<char, char>(Any(), x => x
                .When(c => c == 'a', Match('X'))
                .When(c => c == 'b', Match('Y'))
                .When(c => c == 'c', Match('Z'))
            );
            var result = target.Parse(test);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? 2 : 0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var first = Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = ChainWith<char, char>(first, config => config
                .When(c => c == 'a', x)
                .When(c => c == 'b', y)
                .When(c => c == 'c', z)
            );

            var result = target.GetChildren().ToList();
            result.Count.Should().Be(4);
            result.Should().Contain(first);
            result.Should().Contain(x);
            result.Should().Contain(y);
            result.Should().Contain(z);
        }

        [Test]
        public void ToBnf_1Child()
        {
            var target = ChainWith<char, char>(Any(), config => config
                .When(c => c == 'a', Match('B'))
            ).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .->CHAIN('B')");
        }

        [Test]
        public void ToBnf_3Children()
        {
            var first = Any();
            var x = Match('X');
            var y = Match('Y');
            var z = Match('Z');
            var target = ChainWith<char, char>(first, config => config
                .When(c => (char)c == 'a', x)
                .When(c => (char)c == 'b', y)
                .When(c => (char)c == 'c', z)
            ).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .->CHAIN('X' 'Y' 'Z')");
        }
    }

    public class Extension
    {
        [TestCase("aX", true)]
        [TestCase("bY", true)]
        [TestCase("cZ", true)]
        [TestCase("aZ", false)]
        [TestCase("dW", false)]
        public void Parse_Test(string test, bool shouldMatch)
        {
            var target = Any().ChainWith<char, char, char>(x => x
                .When(c => c == 'a', Match('X'))
                .When(c => c == 'b', Match('Y'))
                .When(c => c == 'c', Match('Z'))
            );
            var result = target.Parse(test);
            result.Success.Should().Be(shouldMatch);
            result.Consumed.Should().Be(shouldMatch ? 2 : 0);
        }
    }
}
