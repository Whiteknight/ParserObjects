namespace ParserObjects.Tests.Examples.XML;

public class XmlParserTests
{
    [Test]
    public void SingleTag()
    {
        var node = XmlParser.Parse("<test></test>");
        node.Should().NotBeNull();
        node.Name.Should().Be("test");
    }

    [Test]
    public void ChildTags()
    {
        var node = XmlParser.Parse("<testa><testb><testc></testc></testb></testa>");
        node.Should().NotBeNull();
        node.Name.Should().Be("testa");
        node.Children.Count.Should().Be(1);
        var child1 = node.Children[0];
        child1.Name.Should().Be("testb");
        child1.Children.Count.Should().Be(1);
        var child2 = child1.Children[0];
        child2.Name.Should().Be("testc");
    }
}
