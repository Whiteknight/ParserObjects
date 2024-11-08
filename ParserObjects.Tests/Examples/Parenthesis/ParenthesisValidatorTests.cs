using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Examples.Parenthesis;

public class ParenthesisValidatorTests
{
    [TestCase("", true)]
    [TestCase("()", true)]
    [TestCase("[]", true)]
    [TestCase("{}", true)]
    [TestCase("([{}])", true)]
    [TestCase("()[]{}", true)]
    [TestCase("()()()", true)]
    [TestCase("((()))", true)]
    [TestCase("[[[]]]", true)]
    [TestCase("{{{}}}", true)]
    [TestCase("({}[])", true)]
    [TestCase("(}", false)]
    [TestCase("(]", false)]
    [TestCase("[}", false)]
    public void Test(string input, bool expected)
    {
        var parser = ParenthesisGrammar.CreateParser();
        var sequence = FromString(input);
        var result = parser.Parse(sequence);
        result.Success.Should().Be(expected);
    }
}
