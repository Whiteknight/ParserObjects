namespace ParserObjects.Tests.Examples.SExpr;

public class SExpressionParserTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = new SExpressionParser();
        var node = parser.Parse("(first 14 \"test\")");
        node.Should().BeOfType<ExpressionNode>();
        var expr = node as ExpressionNode;

        (expr.Children[0] as AtomNode).Type.Should().Be(ValueType.Symbol);
        (expr.Children[0] as AtomNode).Value.Should().Be("first");

        (expr.Children[1] as AtomNode).Type.Should().Be(ValueType.Number);
        (expr.Children[1] as AtomNode).Value.Should().Be(14);

        (expr.Children[2] as AtomNode).Type.Should().Be(ValueType.QuotedString);
        (expr.Children[2] as AtomNode).Value.Should().Be("\"test\"");
    }

    [Test]
    public void Parse_MissingRightParen()
    {
        var parser = new SExpressionParser();
        var node = parser.Parse("(");
        node.Should().BeOfType<ExpressionNode>();
        node.Diagnostics.Count.Should().Be(1);
        node.Diagnostics[0].Should().Be("Missing close parenthesis");
    }
}
