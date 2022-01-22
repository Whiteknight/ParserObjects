using ParserObjects.Visitors;

namespace ParserObjects.Tests.Examples.ExprCalculator;

public class ExpressionGrammarTests
{
    [Test]
    public void GetParserList()
    {
        var parser = ExpressionGrammar.CreateParser();
        var parsers = new ListParsersVisitor().Visit(parser);
        // TODO: Some kind of assertion here
    }
}
